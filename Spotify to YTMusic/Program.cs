using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System.Buffers.Text;
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
            /*            string trackName = "别让爱凋落（Mylove - 请别让爱凋落）";
                        string trackName2 = "SLUMP -Japanese ver.-";
                        string s = "";
                        if (trackName2.Contains('-'))
                        {
                            s = trackName2.Replace("-", " ");
                        }
                        string artist = "卢润泽 ";
                        string artist2 = "Stray Kids";
                        string url = $"https://www.youtube.com/results?search_query={HttpUtility.UrlEncode(s)}+by+{HttpUtility.UrlEncode(artist2)}+%22topic%22";
                        Console.WriteLine(url);*/

        }

    }
}
