using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Web;

namespace Spotify_to_YTMusic
{
    internal class Program

    {
        static async Task Main(string[] args)
        {
            TUI ui = new TUI();
            await ui.Init();
            

            //HttpClient client = new HttpClient();
            //SpotifyApi spotifyApi = new SpotifyApi(client);
            //await spotifyApi.GetAccessTokenAsync();


        }

    }
}
