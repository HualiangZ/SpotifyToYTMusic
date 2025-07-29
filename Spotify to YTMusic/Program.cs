using Spotify_to_YTMusic.Components;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spotify_to_YTMusic
{
    internal class Program
    {
        static void Main(string[] args)
        {
            YoutubeVideoIDFinder.StoreHTML("https://www.youtube.com/results?search_query=jump+by+blackpink+%22topic%22");
            var videoID = YoutubeVideoIDFinder.GetVideoId();
            Console.WriteLine(videoID);
            Console.ReadKey();
        }

        
    }
}
