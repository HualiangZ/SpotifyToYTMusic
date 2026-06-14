using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using Spotify_to_YTMusic.Config;
using Spotify_to_YTMusic.Interfaces;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("SpotifyToTYMusicTest")]

namespace Spotify_to_YTMusic.Components
{
    public class SpotifyApi : ISpotifyApi
    {
        public string AccessToken { get; set; }
        private readonly HttpClient client = new HttpClient();
        MyJsonReader jsonReader = new MyJsonReader();
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        private string RedirectURL { get; set; }
        private string RefreshToken { get; set; }
        private static SpotifyApi _instance;
        private static readonly object padLock = new object();
        private SpotifyApi()
        {
            jsonReader.File = "config.json";
            RedirectURL = "http://127.0.0.1:8888/callback";

        }

        public static SpotifyApi Instance()
        {
            if (_instance == null)
            {
                lock (padLock)
                {
                    _instance = new SpotifyApi();
                }
            }
            return _instance;
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

        public async Task GetAccessTokenAsync()
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
                    string errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine($"Spotify API error: {(int)response.StatusCode} {response.ReasonPhrase} - {errorBody}");
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
                    string errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine($"Spotify API error: {(int)response.StatusCode} {response.ReasonPhrase} - {errorBody}");
                    return null;
                }
            }
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            JObject data = JObject.Parse(json);
            SpotifyPlaylistsModels playlist = new SpotifyPlaylistsModels();
            playlist.Name = name;
            playlist.SnapshotID = data["snapshot_id"].ToString();
            playlist.PlaylistID = data["id"].ToString();
            await MusicDBApi.PostSpotifyPlaylist(playlist);
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
                    string errorBody = await responseMessage.Content.ReadAsStringAsync();
                    Console.WriteLine($"Spotify API error: {(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase} - {errorBody}");
                    return null;
                }
            }

            string json = await responseMessage.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(json);
            var sportifyPlaylist = new SpotifyPlaylistsModels();
            sportifyPlaylist.PlaylistID = playlistId;
            sportifyPlaylist.Name = data["name"].ToString();
            sportifyPlaylist.SnapshotID = await GetPlaylistSnapshotIdAsync(playlistId);
            await MusicDBApi.PostSpotifyPlaylist(sportifyPlaylist);
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
                    string errorBody = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine($"Spotify API error: {(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase} - {errorBody}");
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
            var result = await MusicDBApi.GetOneSportifyPlaylists(playlistId);
            if (result.Playlist == null)
            {
                Console.WriteLine("No Spotify playlist is stored");
                return false;
            }
            string storedSnapshotId = result.Playlist.SnapshotID;
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
                return false;
            }

            if (storedSnapshotId != newSnapshotId)
            {
                await MusicDBApi.UpdateSpotifyPlaylistSnapshotID(playlistId, newSnapshotId);
                return true;
            }
            return false;

        }

        public async Task<JObject> GetTracksInPlaylist(string url)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            if(url == null)
            {
                return null;
            }
            HttpResponseMessage responseMessage = await client.GetAsync(url);
            if (!responseMessage.IsSuccessStatusCode)
            {
                await RefreshAccessToken();
                responseMessage = await client.GetAsync(url);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine(await responseMessage.Content.ReadAsStringAsync());
                    return null;
                }
            }

            Console.WriteLine("Storing Song to Database please wait....");

            string json = await responseMessage.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(json);
            return data;
        }

        public async Task<List<SpotifyTracks>> StorePlaylistInfoToDBAsync(string playlistId)
        {
            string url = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks?limit=100&offset=0";
            var existingTracksResult = await MusicDBApi.GetAllSpotifyTrackInPlaylist(playlistId);
            var existingTrackIDs = existingTracksResult.Tracks ?? new List<string>();
            List<string> newTrackIDs = new List<string>();
            List<SpotifyTracks> spotifyTracksToAdd = new List<SpotifyTracks>();
            List<SpotifyPlaylistTracks> playlistTracksToAdd = new List<SpotifyPlaylistTracks>();

            while (!string.IsNullOrEmpty(url))
            {
                JObject data = await GetTracksInPlaylist(url) ?? throw new Exception("Failed to get tracks from playlist");
                var items = data["items"];

                if (items == null || !items.Any())
                {
                    Console.WriteLine("Playlist empty");
                    break;
                }

                foreach (var item in items)
                {
                    string trackId = item["track"]["id"]?.ToString();
                    if (string.IsNullOrEmpty(trackId))
                    {
                        Console.WriteLine("Skipping item: no valid track ID (local track / podcast episode)");
                        continue;
                    }

                    newTrackIDs.Add(trackId);

                    if (existingTrackIDs.Contains(trackId))
                        continue;

                    spotifyTracksToAdd.Add(new SpotifyTracks
                    {
                        TrackID = trackId,
                        TrackName = item["track"]["name"]?.ToString(),
                        ArtistName = item["track"]["artists"]?[0]?["name"]?.ToString()
                    });

                    playlistTracksToAdd.Add(new SpotifyPlaylistTracks
                    {
                        TrackID = trackId,
                        PlaylistID = playlistId
                    });
                }

                url = data["next"]?.ToString();
            }

            if (spotifyTracksToAdd.Count > 0)
                await MusicDBApi.PostSpotifyTracks(spotifyTracksToAdd);

            if (playlistTracksToAdd.Count > 0)
                await MusicDBApi.PostSpotifyTracksToPlaylist(playlistTracksToAdd);

            await DeleteTracksFromSQLPlaylist(existingTrackIDs, newTrackIDs, playlistId);

            return spotifyTracksToAdd;
        }

        public async Task<(SpotifyTracks spotifyTracks, SpotifyPlaylistTracks playlistTracks)> AddTracksToSQLPlaylist(string _trackName, string _artist, string _trackID, List<string> spotifyPlaylistTracks, string playlistId)
        {
            string trackName = _trackName;
            string artist = _artist;
            string trackID = _trackID;
            
       
            if (spotifyPlaylistTracks == null || !spotifyPlaylistTracks.Contains(trackID))
            {
                SpotifyTracks spotifyTracks = new SpotifyTracks();
                spotifyTracks.TrackID = trackID;
                spotifyTracks.TrackName = trackName;
                spotifyTracks.ArtistName = artist;

                SpotifyPlaylistTracks playlistTracks = new SpotifyPlaylistTracks();
                playlistTracks.TrackID = trackID;
                playlistTracks.PlaylistID = playlistId;
                return (spotifyTracks, playlistTracks);
            }
            return (null, null);
        }

        private async Task DeleteTracksFromSQLPlaylist(List<string> oldTracks, List<string> newTracks, string playlistId)
        {
            if (oldTracks == null || oldTracks.Count == 0)
                return;

            foreach (var item in oldTracks)
            {
                if (newTracks == null || !newTracks.Contains(item))
                {
                    await MusicDBApi.DeleteSpotifyTrackFromPlaylist(new SpotifyPlaylistTracks
                    {
                        PlaylistID = playlistId,
                        TrackID = item
                    });
                    await MusicDBApi.DeleteSpotifyTrack(item);
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

            if(trackIDs.Length == 0)
            {
                return null;
            }

            List<string> trackUriList = new List<string>();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var uris = trackIDs.Select(id => new
            {
                uri = $"spotify:track:{id}"
            }).ToArray();

            var body = new
            {
                tracks = uris
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
                foreach(var id in trackIDs)
                {
                    SpotifyPlaylistTracks track = new SpotifyPlaylistTracks();
                    track.PlaylistID = playlistId;
                    track.TrackID = id;
                    await MusicDBApi.DeleteSpotifyTrackFromPlaylist(track);
                    await MusicDBApi.DeleteSpotifyTrack(id);
                }
                JObject data = JObject.Parse(json);
                Console.WriteLine("Track removed successfully!");
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
            var chunks = trackIDs.Chunk(100);
            List<Task> tasks = new List<Task>();
            foreach (var chunk in chunks)
            {
               tasks.Add(AddTrackToPlaylistchunk(playlistId, chunk));
            }
            await Task.WhenAll(tasks);  
            return await GetPlaylistSnapshotIdAsync(playlistId);
        }

        //need a way to handle more than 100 track to be added
        public async Task<string> AddTrackToPlaylistchunk(string playlistId, string[] trackIDs)
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

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var body = new
            {
                uris = trackUriList,
            };
            string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks", content);
            string json = await response.Content.ReadAsStringAsync();
            List<SpotifyPlaylistTracks> tracks = new List<SpotifyPlaylistTracks>();
            if (response.IsSuccessStatusCode)
            {
                JObject data = JObject.Parse(json);
                Console.WriteLine("Track addded successfully!");

                foreach (var id in trackIDs) 
                {
                    SpotifyPlaylistTracks track = new SpotifyPlaylistTracks();
                    track.PlaylistID = playlistId;
                    track.TrackID = id;
                    tracks.Add(track);
                }
                await MusicDBApi.PostSpotifyTracksToPlaylist(tracks);
                return data["snapshot_id"].ToString();
            }
            else
            {
                Console.WriteLine($"Error add track: {response.StatusCode}\n{json}");
                return null;
            }
        }

        public async Task<SpotifyTracks> SearchForTracks(string _trackName, string artistName)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            string encodedQuery = Uri.EscapeDataString($"{_trackName} {artistName}");
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
            await MusicDBApi.PostSpotifyTrack(spotifyTracks);
            return spotifyTracks;
        }

    }
}
