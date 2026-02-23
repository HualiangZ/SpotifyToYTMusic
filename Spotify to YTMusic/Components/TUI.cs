using Google.Apis.YouTube.v3;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Spotify_to_YTMusic.Components
{
    public class TUI
    {
        static SpotifyApi spotifyApi;
        static YoutubeApi youtubeApi;
        SpotifyToYouTubeSync playlistSync;
        HttpClient client = new HttpClient();
        private string userResponce;
        public TUI()
        {
            spotifyApi = new SpotifyApi(client);
            youtubeApi = new YoutubeApi();
            playlistSync = new SpotifyToYouTubeSync(youtubeApi, spotifyApi);
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
            Console.WriteLine("3. Manual sync Playlist");
            Console.WriteLine("4. Add new spotify playlist to database");
            Console.WriteLine("5. Add new Youtube Music playlist to database");
            userResponce = Console.ReadLine().Trim();
            switch (Int64.Parse(userResponce)) 
            {
                case 1:
                    await SyncSpotifyToYouTubePlaylistAsync().ConfigureAwait(false);
                    break;
                case 2:
                    await SyncYouTubeToSpotifyPlaylistAsync().ConfigureAwait(false);
                    break;
                case 3:
                    await ManualSyncPlaylist();
                    break;
                case 4:
                    await AddSpotifyPlaylistToDB(null);
                    break;
                case 5:
                    await AddYTPlaylistToDB(null);
                    break;
                default:
                    Console.WriteLine("Please enter a number between 1-5");
                    await MenuAsync();
                    break;
            }


        }

        private async Task ManualSyncPlaylist()
        {
            string spotifyPlaylistId = "";
            string youtubePlaylistId = "";
            Console.WriteLine("Enter Spotify Playlist ID");
            spotifyPlaylistId = Console.ReadLine().Trim();
            Console.WriteLine("Enter YouTube Music Playlist ID");
            youtubePlaylistId = Console.ReadLine().Trim();

            await AddSpotifyPlaylistToDB(spotifyPlaylistId);
            await AddYTPlaylistToDB(youtubePlaylistId);
            MusicDBApi.PostPlaylistSync(youtubePlaylistId, spotifyPlaylistId);
        }

        private async Task AddYTPlaylistToDB(string? playlistId)
        {
            if(playlistId == null)
            {
                Console.WriteLine("Enter YouTube Music Playlist ID");
                playlistId = Console.ReadLine().Trim();
            }
            await youtubeApi.StoreYouTubePlaylistToSQL(playlistId);
            await youtubeApi.StoreYTPlaylistTracksToDB(playlistId);
            await MenuAsync();
        }

        private async Task AddSpotifyPlaylistToDB(string? playlistId)
        {
            if (playlistId == null)
            {
                Console.WriteLine("Enter Spotify Playlist ID");
                playlistId = Console.ReadLine().Trim();
            }
            string url = "";
            var spotifyPlaylistTracks = MusicDBApi.GetAllSpotifyTrackInPlaylist(playlistId);
            do
            {
                var data = await spotifyApi.GetTracksInPlaylist(url, playlistId);
                var items = data["items"];

                if (items == null || items.Count() == 0)
                {
                    Console.WriteLine("Playlist empty");
                    break;
                }
                foreach (var item in items)
                {
                    spotifyApi.AddTracksToSQLPlaylist
                        (
                        item["track"]["name"].ToString(),
                        item["track"]["artists"][0]["name"].ToString(),
                        item["track"]["id"].ToString(),
                        spotifyPlaylistTracks.Tracks,
                        playlistId,
                        false
                        );
                }
                url = data["next"].ToString();
            } while (url != "" || url != null);
            await MenuAsync().ConfigureAwait(false);
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

    }
}
