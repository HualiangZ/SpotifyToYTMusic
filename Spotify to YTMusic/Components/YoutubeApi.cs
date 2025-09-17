using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
namespace Spotify_to_YTMusic.Components
{
    internal class YoutubeApi
    {
        UserCredential credential;
        YouTubeService youtubeService;

        public async Task GetCredential()
        {
            using(var stream = new FileStream("YT_client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });
        }

        public async Task<string> CreateNewPlaylist(string playlistName)
        {
            var newPlaylist = new Playlist();
            newPlaylist.Snippet = new PlaylistSnippet();
            newPlaylist.Snippet.Title = playlistName;
            newPlaylist.Snippet.Description = "A playlist created with the YouTube API";
            newPlaylist.Status = new PlaylistStatus();
            newPlaylist.Status.PrivacyStatus = "public";
            int retry = 1;
            while (retry != 0) 
            {
                try
                {
                    await youtubeService.Playlists.Insert(newPlaylist, "snippet,status").ExecuteAsync().ConfigureAwait(false);
                    return newPlaylist.Id;
                }
                catch (Exception ex)
                {
                    await GetCredential().ConfigureAwait(false);
                    retry--;
                    Console.WriteLine(retry);
                }
            }

            Console.WriteLine("Cannot connect to API");
            return "";
        }

        public async Task AddToPlaylist(string playlistId, string videoId)
        {
            if(playlistId == "")
            {
                Console.WriteLine("Playlist ID is empty");
                return;
            }
            if(videoId == "")
            {
                Console.WriteLine("Video ID is empty");
            }

            int retry = 1;

            var playlist = new PlaylistItem();
            playlist.Snippet = new PlaylistItemSnippet();
            playlist.Snippet.PlaylistId = playlistId;
            playlist.Snippet.ResourceId = new ResourceId();
            playlist.Snippet.ResourceId.Kind = "youtube#video";
            playlist.Snippet.ResourceId.VideoId = videoId;

            while(retry != 0)
            {
                try
                {
                    await youtubeService.PlaylistItems.Insert(playlist, "snippet").ExecuteAsync().ConfigureAwait(false);
                    return;
                }
                catch (Exception ex)
                {
                    await GetCredential().ConfigureAwait(false);
                    retry--;
                    Console.WriteLine(retry);
                }
            }

            Console.WriteLine("Invalid Video ID or Playlist ID");
        }

        public async Task DeleteItemFromPlaylistAsync(string playlistId, string videoId)
        {
            if (playlistId == "")
            {
                Console.WriteLine("Playlist ID is empty");
                return;
            }
            if (videoId == "")
            {
                Console.WriteLine("Video ID is empty");
            }

            int retry = 1;

            var playlist = new PlaylistItem();
            playlist.Snippet = new PlaylistItemSnippet();
            playlist.Snippet.PlaylistId = playlistId;

            while (retry != 0)
            {
                try
                {
                    await youtubeService.PlaylistItems.Delete(videoId).ExecuteAsync().ConfigureAwait(false);
                    return;
                }
                catch (Exception ex)
                {
                    await GetCredential().ConfigureAwait(false);
                    retry--;
                    Console.WriteLine(retry);
                }
            }
        }

        public async Task GetItemInPlaylistAsync(string playlistId)
        {
            var nextPageToken = "";
            while (nextPageToken != null) 
            {
                var playlistItemsRequest = youtubeService.PlaylistItems.List("snippet");
                playlistItemsRequest.PlaylistId = playlistId;
                playlistItemsRequest.MaxResults = 50;
                playlistItemsRequest.PageToken = nextPageToken;

                var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync().ConfigureAwait(false);
                foreach(var item in playlistItemsResponse.Items)
                {
                    Console.WriteLine($"{item.Snippet.ResourceId.VideoId}");
                }
                nextPageToken = playlistItemsResponse.NextPageToken;
            } 
            
        }

        public static void StoreTrackToYouTubeDB(string trackName, string artist)
        {
            string url = $"https://www.youtube.com/results?search_query={trackName}+by+{artist}+%22topic%22";
            YouTubeTracks tracks = new YouTubeTracks();
            tracks.TrackName = trackName;
            tracks.ArtistName = artist;
            tracks.TrackID = YoutubeVideoIDFinder.GetVideoId(url);
            MusicDBApi.PostYouTubeTrack(tracks);
        }
    }
}
