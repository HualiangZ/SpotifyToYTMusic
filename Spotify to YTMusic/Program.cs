using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System.Buffers.Text;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Text;

namespace Spotify_to_YTMusic
{
    internal class Program

    {
        static async Task Main(string[] args)
        {
            /*            var videoID = YoutubeVideoIDFinder.GetVideoId("https://www.youtube.com/results?search_query=jump+by+blackpink+%22topic%22");
                        if (videoID != "")
                        {
                            Console.WriteLine(videoID);
                        }
                        else
                        {
                            Console.WriteLine("no ID found");
                        }

                        YoutubeApi api = new YoutubeApi();
                        string playlistId = "PLbqjJZ3RMAtFZhdAnwXI0FJIsrH6rvm9D";
                        await api.GetCredential();
                        //await api.AddToPlaylist(playlistId, videoID);
                        await api.GetItemInPlaylistAsync(playlistId);*/

            /*         HttpClient client = new HttpClient();
                     SpotifyApi api = new SpotifyApi(client);
                     await api.GetAccessTokenAsync().ConfigureAwait(false);
                     await api.GetPlaylistAsync("5a7q5av1kX3ewlMwGuaQE3").ConfigureAwait(false);
                     await api.GetPlaylistSnapshotIdAsync("5a7q5av1kX3ewlMwGuaQE3").ConfigureAwait(false);*/

            SpotifyPlaylistTracks spotifyPlaylistTracks = new SpotifyPlaylistTracks();
            spotifyPlaylistTracks.PlaylistID = "Id2";
            spotifyPlaylistTracks.TrackID = "track1";

            SpotifyPlaylistTracks spotifyPlaylistTracks1 = new SpotifyPlaylistTracks();
            spotifyPlaylistTracks1.PlaylistID = "Id1";
            spotifyPlaylistTracks1.TrackID = "track1";

            MusicDBApi.PostTrackToPlaylist(spotifyPlaylistTracks);
            MusicDBApi.DeleteTrackFromPlaylist(spotifyPlaylistTracks1);
            foreach (var item in MusicDBApi.GetAllTrackInPlaylist("Id1"))
            {
                Console.WriteLine($"{item.PlaylistID}: {item.TrackID}");
            }
        }

    }
}
