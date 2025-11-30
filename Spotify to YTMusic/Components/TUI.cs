using Spotify_to_YTMusic.Components.Sql.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Spotify_to_YTMusic.Components
{
    public class TUI
    {
        SpotifyToYouTubeSync playlistSync = new SpotifyToYouTubeSync();
        private string userResponce;
        public TUI()
        {
            playlistSync.Init().GetAwaiter();
            Task keepRunning = new Task(() =>
            {
                while (true)
                {
                    //Console.WriteLine("sleeping");
                    Thread.Sleep(300000); //5 min sleep
                    //Console.WriteLine("sleep finish");
                    playlistSync.UpdateYTPlaylist().GetAwaiter();
                }
            });
            keepRunning.Start();
        }
        public async Task MenuAsync()
        {
            
            Console.WriteLine("Enter a number depending on what you want to do");
            Console.WriteLine("1. Sync Spotify playlist to YouTube Playlist");
            Console.WriteLine("2. Sync YouTube Playlist to Spotify Playlist");
            Console.WriteLine("3. Update Youtube Playlist");
            Console.WriteLine("4. Update Spotify Playlist");
            userResponce = Console.ReadLine();
            if(userResponce == "1")
            {
                await SyncSpotifyToYouTubePlaylistAsync().ConfigureAwait(false);
            }
            else if(userResponce == "2")
            {
                await SyncYouTubeToSpotifyPlaylistAsync().ConfigureAwait(false);
            }
            else if(userResponce == "3")
            {
                await UpdatingYouTubePlaylistAsync().ConfigureAwait(false);
            }
            else if(userResponce == "4")
            {
                await UpdateSpotifyPlaylist().ConfigureAwait(false);
            }
            else
            {
                Console.WriteLine("Please enter a number between 1-4");
                await MenuAsync();
            }

        }
        private async Task SyncSpotifyToYouTubePlaylistAsync()
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

        private async Task SyncYouTubeToSpotifyPlaylistAsync()
        {
            Console.WriteLine("Enter YouTube Playlist ID");
            string YTPlaylistId = Console.ReadLine();
            bool success = await playlistSync.SyncPlaylistAsyncWithYTID(YTPlaylistId).ConfigureAwait(false);
            if (success)
            {
                Console.WriteLine($"playlist {YTPlaylistId} has been synced");
            }
            else
            {
                Console.WriteLine($"Unable to sync {YTPlaylistId}");
            }
            await MenuAsync().ConfigureAwait(false);

        }

        private async Task UpdatingYouTubePlaylistAsync()
        {
            Console.WriteLine("Enter Spotify playlist ID");
            string spotifyPlaylistId = Console.ReadLine();
            bool isUpdateComplete = playlistSync.SyncSpotifyTracksToYoutube(spotifyPlaylistId).Result;
            if (isUpdateComplete)
            {
                Console.WriteLine($"{spotifyPlaylistId} has synced with YouTube playlist");
                await MenuAsync().ConfigureAwait(false);
            }
            Console.WriteLine($"{spotifyPlaylistId} is not synced to any YouTube playlist");
            await MenuAsync().ConfigureAwait(false);
        }

        private async Task UpdateSpotifyPlaylist()
        {
            Console.WriteLine("Enter YouTube playlist ID");
            string youtubePlaylistId = Console.ReadLine();
            bool isUpdateComplete = playlistSync.SyncYoutubeTracksToSpotify(youtubePlaylistId).Result;
            if (isUpdateComplete)
            {
                Console.WriteLine($"{youtubePlaylistId} has synced with YouTube playlist");
                await MenuAsync().ConfigureAwait(false);
            }

            Console.WriteLine($"{youtubePlaylistId} is not synced to any YouTube playlist");
            await MenuAsync().ConfigureAwait(false);
        }

    }
}
