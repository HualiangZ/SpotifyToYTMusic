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

        public static string GetVideoId(string url)
        {
            try
            {
                var awaiter = CallURL(url);
                if (awaiter.Result != "")
                {
                    string pattern = "\"videoId\":\"([a-zA-Z0-9_-]{11})\"";
                    Match match = Regex.Match(awaiter.Result, pattern);

                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                    else
                    {
                        return "no Video ID found";
                    }
                }
                return "URL link error";
            }
            catch (Exception ex) 
            {
                return "URL link error";
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
