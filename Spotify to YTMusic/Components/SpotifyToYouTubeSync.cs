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
        static SpotifyApi spotifyApi;
        HttpClient client = new HttpClient();
        public SpotifyToYouTubeSync()
        {
            youtubeApi = new YoutubeApi();
            spotifyApi = new SpotifyApi(client);
        }
        public static async Task SyncPlaylistAsync(string? youtubePlaylistId, string spotifyPlaylistId)
        {
            await spotifyApi.GetAccessTokenAsync().ConfigureAwait(false);
            string playlistName = await spotifyApi.StorePlaylistToDB(spotifyPlaylistId).ConfigureAwait(false);
            await spotifyApi.StorePlaylistInfoToDBAsync(spotifyPlaylistId).ConfigureAwait(false);
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
            List<YouTubeTracks> tracksToBeAdded = MusicDBApi.GetUnsyncedTracksFromSpotify(spotifyPlaylistId);
            string youtubePlaylistID = MusicDBApi.GetSyncedPlaylistWithSpotify(spotifyPlaylistId);
            List<YouTubeTracks> tracksToBeRemoved = MusicDBApi.GetUnsyncedTracksFromYoutube(youtubePlaylistID);
            if (tracksToBeAdded != null) 
            {
                foreach (YouTubeTracks track in tracksToBeAdded) 
                {
                    await youtubeApi.AddToPlaylist(spotifyPlaylistId, track.TrackID).ConfigureAwait(false);
                }
            }

            if(tracksToBeRemoved != null)
            {
                foreach (YouTubeTracks track in tracksToBeAdded)
                {
                    await youtubeApi.DeleteItemFromPlaylistAsync(spotifyPlaylistId, track.TrackID).ConfigureAwait(false);
                }
            }

        }
    }
}
