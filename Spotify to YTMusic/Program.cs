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
           /* SpotifyToYouTubeSync playlistSync = new SpotifyToYouTubeSync();
            await playlistSync.Init();
            await playlistSync.SyncPlaylistAsyncWithSpotifyID("3vzc1IWX4yE5txsMCXxGzS");*/
            TUI ui = new TUI();
            await ui.MenuAsync();

        }

    }
}
