using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace Spotify_to_YTMusic.Components
{
    internal class SpotiftyApi
    {
        public string AccessToken { get; set; }
        private static readonly HttpClient client = new HttpClient();
        JsonReader jsonReader = new JsonReader();
        
        public async Task GetAccessTokenAsync()
        {
            await jsonReader.ReadJson();
            var clientId = jsonReader.SpotiftyClientID;
            var clientSecret = jsonReader.SpotiftyClientSecret;

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

        public async Task RetryGetPlaylistAsync(string PlaylistId)
        {
            await GetAccessTokenAsync().ConfigureAwait(false);
            await GetPlaylistAsync(PlaylistId).ConfigureAwait(false);
        }

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
                string json = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine(responseMessage.StatusCode);
                    if(responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await RetryGetPlaylistAsync(PlaylistId).ConfigureAwait(false);
                    }
                    break;
                }

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
