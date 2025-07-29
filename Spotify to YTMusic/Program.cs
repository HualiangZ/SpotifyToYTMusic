using System.Net;
using System.Threading.Tasks;

namespace Spotify_to_YTMusic
{
    internal class Program
    {
        private static string fileoutput = @"./youtubeText.txt";
        static async Task Main(string[] args)
        {
            string url = "https://www.youtube.com/results?search_query=jump+by+blackpink+%22topic%22";
            var awaiter = CallURL(url);
            if (awaiter.Result != "")
            {
                File.WriteAllText(fileoutput, awaiter.Result);
                Console.WriteLine("output compleated");
                awaiter.Result
            }
            Console.ReadKey();
            
        }

        public static async Task<string> CallURL(string url)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(url);
            return await response;
        }
    }
}
