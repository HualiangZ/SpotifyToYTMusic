using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components;
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
            /*YoutubeVideoIDFinder.StoreHTML("https://www.youtube.com/results?search_query=jump+by+blackpink+%22topic%22");
            var videoID = YoutubeVideoIDFinder.GetVideoId();
            if(videoID != "")
            {
                Console.WriteLine(videoID);
            }
            else
            {
                Console.WriteLine("no ID found");
            }

            Console.ReadKey();*/
            HttpClient client = new HttpClient();
            SpotifyApi api = new SpotifyApi(client);
            await api.GetAccessTokenAsync().ConfigureAwait(false);
            await api.GetPlaylistAsync("5a7q5av1kX3ewlMwGuaQE3").ConfigureAwait(false);
            //await api.GetPlaylistSnapshotIdAsync("5a7q5av1kX3ewlMwGuaQE3").ConfigureAwait(false);
            //await api.StoreSnapshotIdAsync("5a7q5av1kX3ewlMwGuaQE3");
        }

    }
}
