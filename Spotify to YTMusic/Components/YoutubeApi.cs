using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.VisualBasic;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
namespace Spotify_to_YTMusic.Components
{
    public class YoutubeApi
    {
        UserCredential credential;
        YouTubeService youtubeService;

        private static YoutubeApi _instance;
        private static readonly object padlock = new object();

        private YoutubeApi() { }

        public static YoutubeApi Instance()
        {
            if(_instance == null)
            {
                lock (padlock)
                {
                    _instance = new YoutubeApi();
                }
            }
            return _instance;
        }

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
            await GetChannelRequest();
        }

        public async Task<bool> GetChannelRequest()
        {
            int retry = 1;
            while (retry != 0)
            {
                try
                {
                    var channelRequest = youtubeService.Channels.List("contentDetails");
                    channelRequest.Mine = true;
                    var channelResponse = await channelRequest.ExecuteAsync();
                    return true;
                }
                catch
                {
                    await GetCredential();
                    retry--;
                }

            }
            return false;
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
                    var playlistResponse = await youtubeService.Playlists.Insert(newPlaylist, "snippet,status").ExecuteAsync();
                    Console.WriteLine($"{playlistResponse.Id}");
                    await StorePlaylistToDB(playlistName, playlistResponse.Id);
                    return playlistResponse.Id;
                }
                catch
                {
                    await GetCredential();
                    retry--;
                    Console.WriteLine(retry);
                }
            }

            Console.WriteLine("Cannot connect to API");
            return "";
        }

        public async Task<string> AddTrackToPlaylist(string playlistId, string videoId)
        {
            if(playlistId == "")
            {
                Console.WriteLine("Playlist ID is empty");
                return null;
            }
            if(videoId == "")
            {
                Console.WriteLine("Video ID is empty");
            }

            int retry = 1;

            var playlistItem = new PlaylistItem();
            playlistItem.Snippet = new PlaylistItemSnippet();
            playlistItem.Snippet.PlaylistId = playlistId;
            playlistItem.Snippet.ResourceId = new ResourceId();
            playlistItem.Snippet.ResourceId.Kind = "youtube#video";
            playlistItem.Snippet.ResourceId.VideoId = videoId;
            while (retry != 0)
            {
                try
                {
                    var item = await youtubeService.PlaylistItems.Insert(playlistItem, "snippet").ExecuteAsync();
                    var request = youtubeService.Playlists.List("snippet");
                    request.Id = playlistId;
                    var response = await request.ExecuteAsync();
                    YouTubePlaylistTracks youTubeTracks = new YouTubePlaylistTracks();
                    youTubeTracks.PlaylistID = playlistId;
                    youTubeTracks.TrackID = videoId;
                    youTubeTracks.ID = item.Id;
                    await MusicDBApi.PostYTTrackToPlaylist(youTubeTracks);
                    return item.Id;
                }
                catch 
                {
                    await GetCredential();
                    retry--;
                    Console.WriteLine(retry);
                }
            }

            Console.WriteLine("Invalid Video ID or Playlist ID or out of Quotas");
            return null;
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
            

            while (retry != 0)
            {
                try
                {
                    var track = await MusicDBApi.GetTrackFromTYPlaylist(playlistId, videoId);
                    await youtubeService.PlaylistItems.Delete(track.Track.ID).ExecuteAsync();
                    await MusicDBApi.DeleteYTTrackFromPlaylist(track.Track);
                    await MusicDBApi.DeleteYouTubeTrack(videoId);
                    return;
                }
                catch (Exception ex)
                {
                    await GetCredential();
                    retry--;
                    Console.WriteLine(ex);
                }
            }
        }

        //stores youtube platlist to database with its tracks
        public async Task<string> StoreYouTubePlaylistToSQL(string playlistId)
        {
            
            string playlistName = "";

            var playlistRequest = youtubeService.Playlists.List("snippet");
            playlistRequest.Id = playlistId;
            var playlistResponse = await playlistRequest.ExecuteAsync();
            if (playlistResponse != null)
            {
                await StorePlaylistToDB(playlistResponse.Items.FirstOrDefault().Snippet.Title, playlistId);
                playlistName = playlistResponse.Items.FirstOrDefault().Snippet.Title;
            }   
            return playlistName;
            
        }

        public async Task<List<string>> StoreYTPlaylistTracksToDB(string playlistId)
        {
            var nextPageToken = "";
            List<string> trackIDs = new List<string>();
            while (nextPageToken != null)
            {
                var playlistItemsRequest = youtubeService.PlaylistItems.List("snippet");
                playlistItemsRequest.PlaylistId = playlistId;
                playlistItemsRequest.MaxResults = 50;
                playlistItemsRequest.PageToken = nextPageToken;
                var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
                foreach (var item in playlistItemsResponse.Items)
                {
                    await GetTrackTitleAndArtistNameAsync(playlistId, item.Snippet.ResourceId.VideoId, item.Id);
                    trackIDs.Add(item.Snippet.ResourceId.VideoId);
                }
                nextPageToken = playlistItemsResponse.NextPageToken;
            }

            return trackIDs;
        }

        private async Task GetTrackTitleAndArtistNameAsync(string playlistId, string videoId, string Id)
        {
            var request = youtubeService.Videos.List("snippet");
            request.Id = videoId;
            var response = await request.ExecuteAsync();

            if (response.Items.Count > 0)
            {
                var snippet = response.Items[0].Snippet;
                YouTubeTracks tracks = new YouTubeTracks();
                tracks.TrackName = snippet.Title;
                tracks.ArtistName = snippet.ChannelTitle.Replace(" - Topic", "");
                tracks.TrackID = videoId;
                await MusicDBApi.PostYouTubeTrack(tracks);

                YouTubePlaylistTracks youTubePlaylistTracks = new YouTubePlaylistTracks();
                youTubePlaylistTracks.TrackID = videoId;
                youTubePlaylistTracks.PlaylistID = playlistId;
                youTubePlaylistTracks.ID = Id;
                await MusicDBApi.PostYTTrackToPlaylist(youTubePlaylistTracks);
            }
        }

        public async Task StorePlaylistToDB(string playlistName, string playlistId)
        {
            YoutubePlaylistsModel playlist = new YoutubePlaylistsModel();
            playlist.Name = playlistName;
            playlist.PlaylistID = playlistId;
            await MusicDBApi.PostYouTubePlaylists(playlist);
        }

        public static async Task StoreTrackToYouTubeDB(string trackName, string artist)
        {
            string url = $"https://www.youtube.com/results?search_query=%22{HttpUtility.UrlEncode(trackName.ToLower())}%22+by+%22{HttpUtility.UrlEncode(artist.ToLower())}%22+%22Topic%22";
            YouTubeTracks tracks = new YouTubeTracks();
            tracks.TrackName = trackName;
            tracks.ArtistName = artist;
            tracks.TrackID = await YoutubeVideoIDFinder.GetVideoId(url);
            await MusicDBApi.PostYouTubeTrack(tracks);
        }
    }
}
