using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using Spotify_to_YTMusic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    internal class SpotifyApi
    {
        public string AccessToken { get; set; }
        private readonly HttpClient client;
        MyJsonReader jsonReader = new MyJsonReader();
        public SpotifyApi(HttpClient client)
        {
            this.client = client;
            jsonReader.File = "config.json";
        }

        public virtual async Task GetAccessTokenAsync()
        {
            await jsonReader.ReadJsonAsync();
            var clientId = jsonReader.ClientID;
            var clientSecret = jsonReader.ClientSecret;

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

            var response = await client.SendAsync(request).ConfigureAwait(false);
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

        public async Task<string> StorePlaylistToDB(string playlistId)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            string url = $"https://api.spotify.com/v1/playlists/{playlistId}";
            HttpResponseMessage responseMessage = await client.GetAsync(url).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                responseMessage = await RefreshAccessToken(url).ConfigureAwait(false);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine(responseMessage.StatusCode);
                }
            }

            string json = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            JObject data = JObject.Parse(json);
            var sportifyPlaylist = new SpotifyPlaylistsModels();
            sportifyPlaylist.PlaylistID = playlistId;
            sportifyPlaylist.Name = data["name"].ToString();
            sportifyPlaylist.SnapshotID = await GetPlaylistSnapshotIdAsync(playlistId).ConfigureAwait(false);
            MusicDBApi.PostSpotifyPlaylist(sportifyPlaylist);
            return sportifyPlaylist.Name;
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


        public async Task<bool> CheckSnapshotIdChangeAsync(string playlistId)
        {
            string newSnapshotId = await GetPlaylistSnapshotIdAsync(playlistId).ConfigureAwait(false);
            string storedSnapshotId = MusicDBApi.GetOneSportifyPlaylists(playlistId).SnapshotID;
            if (storedSnapshotId == null)
            {
                Console.WriteLine("No Spotify SnapshotID is stored");
                return true;
            }

            if (newSnapshotId == null)
            {
                Console.WriteLine("Playlist doesnt exist");
                return true;
            }

            if (storedSnapshotId == newSnapshotId)
            {
                Console.WriteLine("No changes in playlist");
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

        public async Task<string> GetPlaylistTrackLimitAsync(string PlaylistId)
        {
            string url = $"https://api.spotify.com/v1/playlists/{PlaylistId}/tracks";
            HttpResponseMessage responseMessage = await client.GetAsync(url).ConfigureAwait(false);
            if (!responseMessage.IsSuccessStatusCode)
            {
                responseMessage = await RefreshAccessToken(url).ConfigureAwait(false);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine(responseMessage.StatusCode);
                }
            }

            string json = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            JObject data = JObject.Parse(json);

            return data["total"].ToString();
        }

        /*
         * This method does four thing:
         * 1. Store Spotify tracks to DB
         * 2. store Youtube tracks to DB
         * 3. Store Spotify playlist informantion to DB
         * 4. Store what tracks are in Spotify playlist to DB
        */
        public async Task StorePlaylistInfoToDBAsync(string playlistId)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            int limit = 100;
            int offset = 0;
            string url = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks?limit={limit}&offsset={offset}";
            int totalFetched = 0;
            int total = Int32.Parse(await GetPlaylistTrackLimitAsync(playlistId));
            List<string> spotifyPlaylistTracks = MusicDBApi.GetAllSpotifyTrackInPlaylist(playlistId);
            while (url != "null" || url != null)
            {
                HttpResponseMessage responseMessage = await client.GetAsync(url).ConfigureAwait(false);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    responseMessage = await RefreshAccessToken(url).ConfigureAwait(false);

                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        Console.WriteLine(responseMessage.StatusCode); break;
                    }
                }

                string json = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                JObject data = JObject.Parse(json);
                var items = data["items"];
                List<string> IDs = new List<string>();
                if (items == null || items.Count() == 0)
                {
                    Console.WriteLine("Playlist empty");
                    break;
                }
                //add new tracks to DB
                foreach (var item in items)
                {
                    string trackName = item["track"]["name"].ToString();
                    string artist = item["track"]["artists"][0]["name"].ToString();
                    string trackID = item["track"]["id"].ToString();
                    IDs.Add(trackID);
                    if (!spotifyPlaylistTracks.Contains(trackID))
                    {
                        StoreTracksToSpotiftPlaylistDB(trackID, playlistId);
                        StoreTracksToDB(trackID, trackName, artist);
                    }           
                }
                //delete tracks from DB
                foreach (var item in spotifyPlaylistTracks) 
                {
                    if (!IDs.Contains(item))
                    {
                        SpotifyPlaylistTracks toBeDeleted= new SpotifyPlaylistTracks();
                        toBeDeleted.PlaylistID = playlistId;
                        toBeDeleted.TrackID = item;
                        MusicDBApi.DeleteSpotifyTrackFromPlaylist(toBeDeleted);
                    }
                }

                url = data["next"].ToString();
                totalFetched += items.Count();
                offset += items.Count();
                if (items.Count() < limit)
                {
                    break;
                }
            }//end of loop
        }

        private void StoreTracksToDB(string trackID, string trackName, string artist)
        {
            SpotifyTracks tracks = new SpotifyTracks();
            tracks.TrackID = trackID;
            tracks.TrackName = trackName;
            tracks.ArtistName = artist;
            YoutubeApi.StoreTrackToYouTubeDB(trackName, artist);
            MusicDBApi.PostSpotifyTrack(tracks);
        }

        private void StoreTracksToSpotiftPlaylistDB(string trackID, string playlistId)
        {
            SpotifyPlaylistTracks PlaylistTracks = new SpotifyPlaylistTracks();
            PlaylistTracks.TrackID = trackID;
            PlaylistTracks.PlaylistID = playlistId;
            MusicDBApi.PostSpotifyTrackToPlaylist(PlaylistTracks);
        }
    }
}
