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
        private static readonly HttpClient client = new HttpClient();
        public static async Task<string> GetVideoId(string url)
        {
            try
            {
                var awaiter = await CallURL(url);
                if (awaiter != "")
                {
                    string pattern = "\"videoId\":\"([a-zA-Z0-9_-]{11})\"";
                    Match match = Regex.Match(awaiter, pattern);

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
            catch
            {
                return "URL link error";
            }
        }

        private static async Task<string> CallURL(string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            var response = await client.GetStringAsync(url);
            return response;
        }
    }
}
