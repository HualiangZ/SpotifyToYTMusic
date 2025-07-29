using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spotify_to_YTMusic.Components
{

    internal class YoutubeVideoIDFinder
    {
        private static string fileoutput = @"./youtubeText.html";

        public static void StoreHTML(string url)
        {
            var awaiter = CallURL(url);
            if (awaiter.Result != "")
            {
                File.WriteAllText(fileoutput, awaiter.Result);
                Console.WriteLine("output compleated");
            }
        }

        public static string GetVideoId()
        {
            string html = File.ReadAllText(fileoutput);
            string pattern = "\"videoId\":\"([a-zA-Z0-9_-]{11})\"";

            Match match = Regex.Match(html, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return "";
            }

        }

        private static async Task<string> CallURL(string url)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(url);
            return await response;
        }
    }
}
