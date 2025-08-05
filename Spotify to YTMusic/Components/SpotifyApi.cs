using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("SpotifyToTYMusicTest")]

namespace Spotify_to_YTMusic.Components
{
    
    public class SpotifyApi
    {
        public string AccessToken { get; set; }
        private readonly HttpClient client;
        JsonReader jsonReader = new JsonReader();
        public SpotifyApi(HttpClient client)
        {
            this.client = client;
        }

        public virtual async Task GetAccessTokenAsync()
        {
            await jsonReader.ReadJsonAsync();
            var clientId = jsonReader.SpotifyClientID;
            var clientSecret = jsonReader.SpotifyClientSecret;

            var form = new Dictionary<string, string> 
            {
                {"grant_type", "client_credentials" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token") 
            {
                Content = new FormUrlEncodedContent(form)
            };
            var byteArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                JObject tokenData = JObject.Parse(json);
                AccessToken = tokenData["access_token"].ToString();
                Console.WriteLine($"Access Token: {AccessToken}");
            }
            else
            {
                Console.WriteLine("Error getting token: " + json);
            }

        }

        public async Task<HttpResponseMessage> RefreshAccessToken(string url)
        {
            await GetAccessTokenAsync().ConfigureAwait(false);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            return await client.GetAsync(url).ConfigureAwait(false);
        }
        
        public async Task<string> GetPlaylistSnapshotIdAsync(string playlistId)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            string url = $"https://api.spotify.com/v1/playlists/{playlistId}";
            HttpResponseMessage responseMessage = await client.GetAsync(url).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                responseMessage = await RefreshAccessToken(url).ConfigureAwait(false);

                if (!responseMessage.IsSuccessStatusCode)
                    return null;

            }
            string json = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            JObject data = JObject.Parse(json);
            var snapshot = data["snapshot_id"];
            return snapshot.ToString();
        }

        public async Task StoreSnapshotIdAsync(string playlistId)
        {
            var snapshot = await GetPlaylistSnapshotIdAsync(playlistId).ConfigureAwait(false);
            await jsonReader.WriteSpotifySnapshotIdToJsonAsync(snapshot, playlistId).ConfigureAwait(false);
        }


        public async Task CheckSnapshotIdChangeAsync(string playlistId)
        {
            string newSnapshotId = await GetPlaylistSnapshotIdAsync(playlistId).ConfigureAwait(false);
            string storedSnapshotId = await jsonReader.GetPlaylistSnapshotIdAsync(playlistId).ConfigureAwait (false);
            if(storedSnapshotId == null)
            {
                Console.WriteLine("No Spotify SnapshotID is stored");
                return;
            }

            if(newSnapshotId == null)
            {
                Console.WriteLine("Playlist doesnt exist");
                return;
            }

            if (storedSnapshotId == newSnapshotId)
            {
                Console.WriteLine("No changes in playlist");
                return;
            }

            if(storedSnapshotId != newSnapshotId)
            {
                //might need changing down the line no sure yet
                //await GetPlaylistAsync(playlistId).ConfigureAwait(false);
                await StoreSnapshotIdAsync(playlistId).ConfigureAwait(false);
            }

        }

        //change this to return a file with all the music name and artist.
        public async Task GetPlaylistAsync(string PlaylistId)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            int limit = 100;
            int offset = 0;
            int totalFetched = 0;
            while (totalFetched < 200)
            {
                string url = $"https://api.spotify.com/v1/playlists/{PlaylistId}/tracks?limit{limit}&offsset={offset}";
                HttpResponseMessage responseMessage = await client.GetAsync(url).ConfigureAwait(false);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    responseMessage = await RefreshAccessToken(url).ConfigureAwait(false);

                    if (!responseMessage.IsSuccessStatusCode) 
                    {
                        Console.WriteLine(responseMessage.StatusCode);
                        break;
                    }
                }

                string json = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                JObject data = JObject.Parse(json);
                var items = data["items"];
                var total = data["total"];
                if (items == null || items.Count() == 0)
                {
                    Console.WriteLine("Playlist empty");
                    break;
                }

                foreach (var item in items) 
                {
                    string trackName = item["track"]["name"].ToString();
                    string artist = item["track"]["artists"][0]["name"].ToString();
                    Console.WriteLine($"{trackName} by {artist}");
                }
                Console.WriteLine(total);
                totalFetched += items.Count();
                offset += items.Count();
                if(items.Count() < limit)
                {
                    break;
                }

            }
        }
    }
}
