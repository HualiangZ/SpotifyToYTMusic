using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotify_to_YTMusic.Components
{
    internal class SpotifyToYouTubeSync
    {
        public static async Task SyncPlaylistAsync(string? youtubePlaylistId, string SpotifyPlaylistId)
        {
            HttpClient client = new HttpClient();
            SpotifyApi spotifyAPI = new SpotifyApi(client);
            await spotifyAPI.GetAccessTokenAsync().ConfigureAwait(false);
            await spotifyAPI.StorePlaylistToDB(SpotifyPlaylistId).ConfigureAwait(false);
            await spotifyAPI.StorePlaylistInfoToDBAsync(SpotifyPlaylistId).ConfigureAwait(false);

            if (youtubePlaylistId != null)
            {
                YoutubeApi youtubeAPI = new YoutubeApi();
                string playlistId = youtubePlaylistId;
                await youtubeAPI.GetCredential();
            }

        }
    }
}
