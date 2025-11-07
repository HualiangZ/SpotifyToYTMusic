using Spotify_to_YTMusic.Components.Sql.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if(userResponce == "3")
            {
                UpdatingYouTubePlaylist();
            }
        }
        public string UpdatingYouTubePlaylist()
        {
            Console.WriteLine("Enter Spotify playlist ID");
            string spotifyPlaylistId = Console.ReadLine();
            bool isUpdateComplete = playlistSync.SyncSpotifyTracksToYoutube(spotifyPlaylistId).Result;
            if (isUpdateComplete)
            {
                return $"{spotifyPlaylistId} is not synced to any YouTube playlist";
            }

            return $"{spotifyPlaylistId} has synced with YouTube playlist";

        }

        public async Task<string> SyncSpotifyToYouTubePlaylistAsync()
        {
            Console.WriteLine("Enter Spotify playlist ID");
            string spotifyPlaylistId = Console.ReadLine();
            await playlistSync.SyncPlaylistAsyncWithSpotifyID(spotifyPlaylistId).ConfigureAwait(false);
            return $"playlist {spotifyPlaylistId} has been synced";
        }

        
    
    }
}
