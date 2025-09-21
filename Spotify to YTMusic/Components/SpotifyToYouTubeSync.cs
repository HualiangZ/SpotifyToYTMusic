using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotify_to_YTMusic.Components
{
    internal class SpotifyToYouTubeSync
    {
        static YoutubeApi youtubeApi;
        public SpotifyToYouTubeSync()
        {
            youtubeApi = new YoutubeApi();
        }
        public static async Task SyncPlaylistAsync(string? youtubePlaylistId, string spotifyPlaylistId)
        {

            HttpClient client = new HttpClient();
            SpotifyApi spotifyAPI = new SpotifyApi(client);
            await spotifyAPI.GetAccessTokenAsync().ConfigureAwait(false);
            string playlistName = await spotifyAPI.StorePlaylistToDB(spotifyPlaylistId).ConfigureAwait(false);

            if (youtubePlaylistId != null)
            {
                string playlistId = youtubePlaylistId;
                await youtubeApi.GetCredential();
                string newYoutubePlaylistId = await youtubeApi.CreateNewPlaylist(playlistName).ConfigureAwait(false);
                PlaylistSync newPlaylistSync = new PlaylistSync();
                newPlaylistSync.SpotifyPlaylistID = spotifyPlaylistId;
                newPlaylistSync.YTPlaylistID = newYoutubePlaylistId;
                MusicDBApi.PostPlaylistSync(newPlaylistSync);
                return;
            }

            PlaylistSync playlistSync = new PlaylistSync();
            playlistSync.SpotifyPlaylistID = spotifyPlaylistId;
            playlistSync.YTPlaylistID = youtubePlaylistId;
            MusicDBApi.PostPlaylistSync(playlistSync);

        }

        public static async Task SyncYoutubeTracksToSpotify(string spotifyPlaylistId)
        {
            List<YouTubeTracks> tracks = MusicDBApi.GetUnsyncedTracks(spotifyPlaylistId);
            string youtubePlaylistID = MusicDBApi.GetSyncedPlaylistWithSpotify(spotifyPlaylistId);
            if (tracks == null) 
            {
                Console.WriteLine("No missing tracks");
                return; 
            }

            foreach (YouTubeTracks track in tracks) 
            {
                await youtubeApi.AddToPlaylist(spotifyPlaylistId, track.TrackID).ConfigureAwait(false);
            }
        }
    }
}
