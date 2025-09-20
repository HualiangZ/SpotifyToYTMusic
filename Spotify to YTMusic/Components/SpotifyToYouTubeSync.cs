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
        public static async Task SyncPlaylistAsync(string? youtubePlaylistId, string spotifyPlaylistId)
        {

            HttpClient client = new HttpClient();
            SpotifyApi spotifyAPI = new SpotifyApi(client);
            await spotifyAPI.GetAccessTokenAsync().ConfigureAwait(false);
            string playlistName = await spotifyAPI.StorePlaylistToDB(spotifyPlaylistId).ConfigureAwait(false);
            await spotifyAPI.StorePlaylistInfoToDBAsync(spotifyPlaylistId).ConfigureAwait(false);

            if (youtubePlaylistId != null)
            {
                YoutubeApi youtubeAPI = new YoutubeApi();
                string playlistId = youtubePlaylistId;
                await youtubeAPI.GetCredential();
                string newYoutubePlaylistId = await youtubeAPI.CreateNewPlaylist(playlistName).ConfigureAwait(false);
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
    }
}
