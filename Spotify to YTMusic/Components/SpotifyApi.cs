using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using Spotify_to_YTMusic.Config;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Net.WebRequestMethods;

[assembly: InternalsVisibleTo("SpotifyToTYMusicTest")]

namespace Spotify_to_YTMusic.Components
{

    internal class SpotifyApi
    {
        public string AccessToken { get; set; }
        private readonly HttpClient client;
        MyJsonReader jsonReader = new MyJsonReader();
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        private string RedirectURL { get; set; }
        private string RefreshToken { get; set; }
        public SpotifyApi(HttpClient client)
        {
            this.client = client;
            jsonReader.File = "config.json";
            RedirectURL = "http://127.0.0.1:8888/callback";

        }

        public async Task<string> GetAuthCodeAsync()
        {
            await jsonReader.ReadJsonAsync();
            ClientId = jsonReader.ClientID;
            ClientSecret = jsonReader.ClientSecret;
            string scopes = "playlist-modify-public playlist-modify-private";
            string authUrl =
            "https://accounts.spotify.com/authorize?" +
            $"client_id={ClientId}" +
            "&response_type=code" +
            $"&redirect_uri={Uri.EscapeDataString(RedirectURL)}" +
            $"&scope={Uri.EscapeDataString(scopes)}";

            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });

            // Wait for the code after login
            string authorizationCode = await WaitForSpotifyCallback();
            Console.WriteLine($"Got authorization code");
            return authorizationCode;

        }
        private static async Task<string> WaitForSpotifyCallback()
        {
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add("http://127.0.0.1:8888/callback/");
                listener.Start();
                Console.WriteLine("Waiting for Spotify login...");

                var context = await listener.GetContextAsync();
                var request = context.Request;

                string code = request.QueryString["code"];

                // Respond to browser
                string responseString = "<html><body><h1>You may close this window.</h1></body></html>";
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                listener.Stop();
                return code;
            }
        }

        public virtual async Task GetAccessTokenAsync()
        {
            await jsonReader.ReadJsonAsync();
            var form = new Dictionary<string, string>
            {
                {"grant_type", "authorization_code" },
                {"code", await GetAuthCodeAsync()},
                {"redirect_uri", RedirectURL}
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            {
                Content = new FormUrlEncodedContent(form)
            };
            var byteArray = Encoding.ASCII.GetBytes($"{ClientId}:{ClientSecret}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await client.SendAsync(request).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                JObject tokenData = JObject.Parse(json);
                AccessToken = tokenData["access_token"].ToString();
                RefreshToken = tokenData["refresh_token"].ToString();
                Console.WriteLine($"Access Token collected");
            }
            else
            {
                Console.WriteLine("Error getting token: " + json);
            }

        }

        public async Task RefreshAccessToken()
        {
            var data = new Dictionary<string, string>
            {
                {"grant_type", "refresh_token"},
                {"refresh_token", RefreshToken }
            };
            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            {
                Content = new FormUrlEncodedContent(data)
            };
            var byteArray = Encoding.ASCII.GetBytes($"{ClientId}:{ClientSecret}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error refreshing token: " + json);
                return;
            }

            JObject tokenData = JObject.Parse(json);
            AccessToken = tokenData["access_token"].ToString();
            if (tokenData["refresh_token"] != null)
            {
                string newRefreshToken = tokenData["refresh_token"].ToString();
                Console.WriteLine($"Updated Refresh Token: {newRefreshToken}");
            }

        }

        public async Task<string> GetUserID()
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            string url = "https://api.spotify.com/v1/me";
            HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                await RefreshAccessToken().ConfigureAwait(false);
                response = await client.GetAsync(url).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Unable to get Access Token");
                    return null;
                }
            }
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            JObject data = JObject.Parse(json);
            return data["id"].ToString();
        }

        public async Task<SpotifyPlaylistsModels> CreatePlaylist(string name)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            string userId = GetUserID().Result;
            string url = $"https://api.spotify.com/v1/me/playlists";

            var body = new 
            { 
                name = name,
                discription = "new playlist",
                @public = false
            };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                await RefreshAccessToken().ConfigureAwait(false);
                response = await client.PostAsync(url, content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    return null;
                }
            }
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Console.WriteLine(json);
            JObject data = JObject.Parse(json);
            SpotifyPlaylistsModels playlist = new SpotifyPlaylistsModels();
            playlist.Name = name;
            playlist.SnapshotID = data["snapshot_id"].ToString();
            playlist.PlaylistID = data["id"].ToString();
            MusicDBApi.PostSpotifyPlaylist(playlist);
            return playlist;
        }

        public async Task<string> StorePlaylistToDB(string playlistId)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            string url = $"https://api.spotify.com/v1/playlists/{playlistId}";
            HttpResponseMessage responseMessage = await client.GetAsync(url);
            if (!responseMessage.IsSuccessStatusCode)
            {
                await RefreshAccessToken();
                responseMessage = await client.GetAsync(url);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine("Unable to get Access Token");
                    return null;
                }
            }

            string json = await responseMessage.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(json);
            var sportifyPlaylist = new SpotifyPlaylistsModels();
            sportifyPlaylist.PlaylistID = playlistId;
            sportifyPlaylist.Name = data["name"].ToString();
            sportifyPlaylist.SnapshotID = await GetPlaylistSnapshotIdAsync(playlistId);
            MusicDBApi.PostSpotifyPlaylist(sportifyPlaylist);
            return sportifyPlaylist.Name;
        }

        public async Task<string> GetPlaylistSnapshotIdAsync(string playlistId)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            string url = $"https://api.spotify.com/v1/playlists/{playlistId}";
            HttpResponseMessage responseMessage = await client.GetAsync(url).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                await RefreshAccessToken().ConfigureAwait(false);
                responseMessage = await client.GetAsync(url).ConfigureAwait(false);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine("Unable to get Access Token");
                    return null;
                }
            }
            string json = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            JObject data = JObject.Parse(json);
            var snapshot = data["snapshot_id"];
            return snapshot.ToString();
        }


        public async Task<bool> CheckSnapshotIdChangeAsync(string playlistId)
        {
            string newSnapshotId = await GetPlaylistSnapshotIdAsync(playlistId);
            string storedSnapshotId = MusicDBApi.GetOneSportifyPlaylists(playlistId).Playlist.SnapshotID;
            if (storedSnapshotId == null)
            {
                Console.WriteLine("No Spotify SnapshotID is stored");
                return false;
            }

            if (newSnapshotId == null)
            {
                Console.WriteLine("Playlist doesnt exist");
                return false;
            }

            if (storedSnapshotId == newSnapshotId)
            {
                //Console.WriteLine("No changes in playlist");
                return false;
            }

            if (storedSnapshotId != newSnapshotId)
            {
                //might need changing down the line no sure yet
                MusicDBApi.UpdateSpotifyPlaylistSnapshotID(playlistId, newSnapshotId);
                return true;
            }
            return false;

        }

        /*
         * This method does four thing:
         * 1. Store Spotify tracks to DB
         * 2. store Youtube tracks to DB
         * 3. Store Spotify playlist informantion to DB
         * 4. Store what tracks are in Spotify playlist to DB
        */
        public async Task<bool> StorePlaylistInfoToDBAsync(string playlistId)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            int limit = 100;
            int offset = 0;
            string url = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks?limit={limit}&offsset={offset}";
            var spotifyPlaylistTracks = MusicDBApi.GetAllSpotifyTrackInPlaylist(playlistId);
            List<string> IDs = new List<string>();
            while (url != "")
            {
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    await RefreshAccessToken();
                    responseMessage = await client.GetAsync(url);
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Unable to get Access Token");
                        return false;
                    }
                }

                Console.WriteLine("Storing Song to Database please wait....");

                string json = await responseMessage.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(json);
                var items = data["items"];
                
                if (items == null || items.Count() == 0)
                {
                    Console.WriteLine("Playlist empty");
                    break;
                }
                //add new tracks to DB
                foreach (var item in items)
                {
                    AddTracksToPlaylist
                        (
                        item["track"]["name"].ToString(), 
                        item["track"]["artists"][0]["name"].ToString(),
                        item["track"]["id"].ToString(),
                        spotifyPlaylistTracks.Tracks, 
                        playlistId
                        );
                    IDs.Add(item["track"]["id"].ToString());

                }
                url = data["next"].ToString();
            }//end of loop

            //delete tracks from DB
            DeleteTracksFromPlaylist(spotifyPlaylistTracks.Tracks, IDs, playlistId);

            return true;
        }

        private void AddTracksToPlaylist(string _trackName, string _artist, string _trackID, List<string> tracks, string playlistId)
        {
            string trackName = _trackName;
            string artist = _artist;
            string trackID = _trackID;
            
            if (!tracks.Contains(trackID))
            {
                StoreTracksToDB(trackID, trackName, artist);
                StoreTracksToSpotiftPlaylistDB(trackID, playlistId);
                YoutubeApi.StoreTrackToYouTubeDB(trackName, artist);
            }
        }

        private void DeleteTracksFromPlaylist(List<string> oldTracks, List<string> newTracks, string playlistId)
        {
            foreach (var item in oldTracks)
            {
                if (!newTracks.Contains(item))
                {
                    SpotifyPlaylistTracks toBeDeleted = new SpotifyPlaylistTracks();
                    toBeDeleted.PlaylistID = playlistId;
                    toBeDeleted.TrackID = item;
                    MusicDBApi.DeleteSpotifyTrackFromPlaylist(toBeDeleted);
                    MusicDBApi.DeleteSpotifyTrack(item);
                }
            }
        }

        public async Task<string> DeleteTrackFromPlaylist(string playlistId, string[] trackIDs)
        {
            if (trackIDs.Length > 100)
            {
                Console.WriteLine("List of track ID can't be more than 100 items");
                return null;
            }

            List<string> trackUriList = new List<string>();
            foreach (var item in trackIDs)
            {
                string trackUri = $"spotify:track:{item}";
                trackUriList.Add(trackUri);
            }
            string[] trackUriArr = trackIDs.ToArray();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var body = new
            {
                uris = trackUriArr,
            };
            string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.spotify.com/v1/playlists/{playlistId}/tracks")
            {
                Content = content
            };

            var response = await client.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {

                JObject data = JObject.Parse(json);
                Console.WriteLine("Track removed successfully!");
                SpotifyPlaylistTracks track = new SpotifyPlaylistTracks();
                await StorePlaylistInfoToDBAsync(playlistId);
                return data["snapshot_id"].ToString();
            }
            else
            {
                Console.WriteLine($"Error removing track: {response.StatusCode}\n{json}");
                return null;
            }

        }

        public async Task<string> AddTrackToPlaylist(string playlistId, string[] trackIDs)
        {
            if (trackIDs.Length > 100)
            {
                Console.WriteLine("List of track ID can't be more than 100 items");
                return null;
            }
            List<string> trackUriList = new List<string>();
            foreach (var item in trackIDs)
            {
                string trackUri = $"spotify:track:{item}";
                trackUriList.Add(trackUri);
            }
            string[] trackUriArr = trackIDs.ToArray();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var body = new
            {
                uris = trackUriList,
            };
            string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
            Console.WriteLine(jsonBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks", content);
            string json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                JObject data = JObject.Parse(json);
                Console.WriteLine("Track addded successfully!");
                SpotifyPlaylistTracks track = new SpotifyPlaylistTracks();
                await StorePlaylistInfoToDBAsync(playlistId);
                return data["snapshot_id"].ToString();
            }
            else
            {
                Console.WriteLine($"Error add track: {response.StatusCode}\n{json}");
                return null;
            }
        }

        private void StoreTracksToDB(string trackID, string trackName, string artist)
        {
            SpotifyTracks tracks = new SpotifyTracks();
            tracks.TrackID = trackID;
            tracks.TrackName = trackName;
            tracks.ArtistName = artist;
            MusicDBApi.PostSpotifyTrack(tracks);
        }

        private void StoreTracksToSpotiftPlaylistDB(string trackID, string playlistId)
        {
            SpotifyPlaylistTracks PlaylistTracks = new SpotifyPlaylistTracks();
            PlaylistTracks.TrackID = trackID;
            PlaylistTracks.PlaylistID = playlistId;
            MusicDBApi.PostSpotifyTrackToPlaylist(PlaylistTracks);
        }

        public async Task<SpotifyTracks> SearchForTracks(string _trackName, string artistName)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            string encodedQuery = Uri.EscapeDataString($"{_trackName} by {artistName}");
            string url = $"https://api.spotify.com/v1/search?q={encodedQuery}&type=track";
            HttpResponseMessage responseMessage = await client.GetAsync(url);
            if (!responseMessage.IsSuccessStatusCode)
            {
                await RefreshAccessToken();
                responseMessage = await client.GetAsync(url);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine("Unable to get Access Token");
                    return null;
                }
            }
            string json = await responseMessage.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(json);
            var items = data["tracks"]["items"];
            string trackName = items[0]["name"].ToString();
            string artist = items[0]["artists"][0]["name"].ToString();
            string trackID = items[0]["id"].ToString();
            SpotifyTracks spotifyTracks = new SpotifyTracks();
            spotifyTracks.TrackID = trackID;
            spotifyTracks.ArtistName = artistName;
            spotifyTracks.TrackName = trackName;
            MusicDBApi.PostSpotifyTrack(spotifyTracks);
            return spotifyTracks;
        }

    }
}
