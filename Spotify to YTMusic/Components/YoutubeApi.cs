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
                    playlist = await youtubeService.PlaylistItems.Insert(playlist, "snippet").ExecuteAsync().ConfigureAwait(false);
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

    }
}
