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
            HttpClient client = new HttpClient();
            SpotifyApi spotifyAPI = new SpotifyApi(client);
            await spotifyAPI.GetAccessTokenAsync().ConfigureAwait(false);
            //string playlistName = await spotifyAPI.StorePlaylistToDB("3vzc1IWX4yE5txsMCXxGzS").ConfigureAwait(false);
            //await spotifyAPI.StorePlaylistInfoToDBAsync("3vzc1IWX4yE5txsMCXxGzS").ConfigureAwait(false);
            Console.WriteLine(MusicDBApi.GetUnsyncedTracks("3vzc1IWX4yE5txsMCXxGzS").Count);
            foreach (var item in MusicDBApi.GetUnsyncedTracks("3vzc1IWX4yE5txsMCXxGzS"))
            {
                Console.WriteLine(item.TrackID);
            }
        }

    }
}
