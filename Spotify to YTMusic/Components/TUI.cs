using Spotify_to_YTMusic.Components.Sql.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Spotify_to_YTMusic.Components
{
    internal class TUI
    {
        SpotifyToYouTubeSync playlistSync = new SpotifyToYouTubeSync();
        private string userResponce;
        public async Task MenuAsync()
        {
            await playlistSync.Init();
            Console.WriteLine("Select a number depending on what you want to do");
            Console.WriteLine("1. Sync Spotify playlist to YouTube Playlist");
            Console.WriteLine("2. Sync YouTube Playlist to Spotify Playlist");
            Console.WriteLine("3. Update Youtube Playlist");
            Console.WriteLine("4. Update Spotify Playlist");
            userResponce = Console.ReadLine();
            if(userResponce == "1")
            {
                await SyncSpotifyToYouTubePlaylistAsync().ConfigureAwait(false);
            }
            if(userResponce == "2")
            {
                await SyncYouTubeToSpotifyPlaylistAsync().ConfigureAwait(false);
            }
            if(userResponce == "3")
            {
                await UpdatingYouTubePlaylistAsync().ConfigureAwait(false);
            }
            if(userResponce == "4")
            {

            }
        }
        public async Task SyncSpotifyToYouTubePlaylistAsync()
        {
            Console.WriteLine("Enter Spotify playlist ID");
            string spotifyPlaylistId = Console.ReadLine();
            bool success = await playlistSync.SyncPlaylistAsyncWithSpotifyID(spotifyPlaylistId).ConfigureAwait(false);
            if (success)
            {
                Console.WriteLine($"playlist {spotifyPlaylistId} has been synced");
            }else
            {
                Console.WriteLine($"Unable to sync {spotifyPlaylistId}");
            }

            await MenuAsync().ConfigureAwait(false);
        }

        public async Task SyncYouTubeToSpotifyPlaylistAsync()
        {
            Console.WriteLine("Enter YouTube Playlist ID");
            string YTPlaylistId = Console.ReadLine();
            await playlistSync.SyncPlaylistAsyncWithYTID(YTPlaylistId).ConfigureAwait(false);

            Console.WriteLine($"playlist {YTPlaylistId} has been synced");
            await MenuAsync().ConfigureAwait(false);
        }

        public async Task UpdatingYouTubePlaylistAsync()
        {
            Console.WriteLine("Enter Spotify playlist ID");
            string spotifyPlaylistId = Console.ReadLine();
            bool isUpdateComplete = playlistSync.SyncSpotifyTracksToYoutube(spotifyPlaylistId).Result;
            if (isUpdateComplete)
            {
                await MenuAsync().ConfigureAwait(false);
                Console.WriteLine($"{spotifyPlaylistId} is not synced to any YouTube playlist");
            }
            Console.WriteLine($"{spotifyPlaylistId} has synced with YouTube playlist");
        }


    }
}
