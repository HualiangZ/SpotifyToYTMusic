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
            HttpClient client = new HttpClient();
            SpotifyApi ui = new SpotifyApi(client);
            await ui.GetAccessTokenAsync();
            await ui.StorePlaylistInfoToDBAsync("5a7q5av1kX3ewlMwGuaQE3");

            /*            string trackName = "别让爱凋落（Mylove 请别让爱凋落）";
                        string artist = "卢润泽 ";
                        string url = $"https://www.youtube.com/results?search_query={HttpUtility.UrlEncode(trackName)}+by+{HttpUtility.UrlEncode(artist)}+%22topic%22";
                        Console.WriteLine(url);*/

        }

    }
}
