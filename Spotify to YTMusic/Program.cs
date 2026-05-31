using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components;

namespace Spotify_to_YTMusic
{
    internal class Program

    {
        static async Task Main(string[] args)
        {
            TUI ui = new TUI();
            await ui.Init();

        }

    }
}
