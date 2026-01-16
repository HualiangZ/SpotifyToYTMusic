using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System.Buffers.Text;
using System.Diagnostics;
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
            await ui.MenuAsync();
            //Console.WriteLine(MusicDBApi.GetAllSpotifyTrackInPlaylist("5a7q5av1kX3ewlMwGuaQE3").Tracks[0]);
            /*            string trackName = "别让爱凋落（Mylove - 请别让爱凋落）";
                        string trackName2 = "SLUMP -Japanese ver.-";
                        string artist = "卢润泽 ";
                        string artist2 = "Stray Kids";
                        string url = $"https://www.youtube.com/results?search_query=%22{HttpUtility.UrlEncode(trackName2.ToLower())}%22+by+%22{HttpUtility.UrlEncode(artist2.ToLower())}%22+%22Topic%22";
                        Console.WriteLine(url);*/

        }

    }
}
