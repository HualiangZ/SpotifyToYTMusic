using Spotify_to_YTMusic.Components;

namespace Spotify_to_YTMusic
{
    internal class Program

    {
        static async Task Main(string[] args)
        {
            TUI ui = new TUI();
            await ui.MenuAsync();

            /*            string trackName = "别让爱凋落（Mylove 请别让爱凋落）";
                        string artist = "卢润泽 ";
                        string url = $"https://www.youtube.com/results?search_query={HttpUtility.UrlEncode(trackName)}+by+{HttpUtility.UrlEncode(artist)}+%22topic%22";
                        Console.WriteLine(url);*/

        }

    }
}
