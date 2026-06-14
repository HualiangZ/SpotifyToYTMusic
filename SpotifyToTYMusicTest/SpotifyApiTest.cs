using System.Net;
using System.Reflection;
using Spotify_to_YTMusic.Components;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;

namespace SpotifyToTYMusicTest
{
    public class SpotifyApiTest
    {
        private SpotifyApi _api;
        private FieldInfo _clientField;
        private HttpClient _mockHttpClient;
        private HttpMessageHandlerMock _handlerMock;

        [SetUp]
        public void Setup()
        {
            _api = SpotifyApi.Instance();
            _clientField = typeof(SpotifyApi).GetField("client",
                BindingFlags.NonPublic | BindingFlags.Instance);
            _handlerMock = new HttpMessageHandlerMock();
            _mockHttpClient = new HttpClient(_handlerMock);
            _clientField.SetValue(_api, _mockHttpClient);
            _api.AccessToken = "test-access-token";
        }

        [TearDown]
        public void Cleanup()
        {
            _mockHttpClient.Dispose();
            _handlerMock.Dispose();
            _clientField.SetValue(_api, new HttpClient());
            CleanupTestData().GetAwaiter().GetResult();
        }

        private static async Task CleanupTestData()
        {
            try
            {
                using var cnn = new System.Data.SQLite.SQLiteConnection(
                    "Data Source=./MusicDB.db;foreign keys=true;");
                await cnn.OpenAsync();
                using var cmd = cnn.CreateCommand();
                cmd.CommandText = @"DELETE FROM SpotifyPlaylists WHERE PlaylistID IN ('gap_new_playlist','gap_store_playlist','gap_retry_playlist','gap_check_changed','gap_check_same','gap_info_playlist','gap_store_fail');
DELETE FROM SpotifyTracks WHERE TrackID IN ('gap_info_t1','gap_chunk_track_1','gap_chunk_track_2','gap_add_track_1');
DELETE FROM SpotifyPlaylistTracks WHERE PlaylistID IN ('gap_add_playlist','gap_chunk_playlist','gap_info_playlist');";
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }

        private void SetPrivateProperty(string propertyName, object value)
        {
            var fieldName = $"<{propertyName}>k__BackingField";
            var field = typeof(SpotifyApi).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_api, value);
        }

        private void SetupRefreshTokenDependencies()
        {
            SetPrivateProperty("ClientId", "test-client-id");
            SetPrivateProperty("ClientSecret", "test-client-secret");
            SetPrivateProperty("RefreshToken", "test-refresh-token");
        }

        private static HttpResponseMessage TokenResponse(string accessToken = "new-access-token")
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"access_token\": \"{accessToken}\"}}")
            };
        }

        // ==================== GetPlaylistSnapshotIdAsync ====================

        [Test]
        public async Task GetPlaylistSnapshotIdAsync_ReturnsSnapshotId_When200()
        {
            _handlerMock.SetupResponse(HttpStatusCode.OK,
                new StringContent("{\"snapshot_id\": \"test_snapshot_123\"}"));

            var result = await _api.GetPlaylistSnapshotIdAsync("playlist_123");

            Assert.That(result, Is.EqualTo("test_snapshot_123"));
        }

        [Test]
        public async Task GetPlaylistSnapshotIdAsync_ReturnsNull_When400()
        {
            _handlerMock.SetupResponse(HttpStatusCode.BadRequest,
                new StringContent("{\"error\": \"bad request\"}"));

            var result = await _api.GetPlaylistSnapshotIdAsync("playlist_123");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetPlaylistSnapshotIdAsync_ReturnsSnapshotId_AfterTokenRefresh()
        {
            SetupRefreshTokenDependencies();
            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.Unauthorized),
                TokenResponse(),
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"snapshot_id\": \"refreshed_snapshot\"}")
                }
            );

            var result = await _api.GetPlaylistSnapshotIdAsync("playlist_123");

            Assert.That(result, Is.EqualTo("refreshed_snapshot"));
        }

        // ==================== GetUserID ====================

        [Test]
        public async Task GetUserID_ReturnsUserId_When200()
        {
            _handlerMock.SetupResponse(HttpStatusCode.OK,
                new StringContent("{\"id\": \"spotify_user_456\"}"));

            var result = await _api.GetUserID();

            Assert.That(result, Is.EqualTo("spotify_user_456"));
        }

        [Test]
        public async Task GetUserID_ReturnsNull_When400()
        {
            _handlerMock.SetupResponse(HttpStatusCode.BadRequest,
                new StringContent("{\"error\": \"bad\"}"));

            var result = await _api.GetUserID();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetUserID_RetriesAfterTokenRefresh()
        {
            SetupRefreshTokenDependencies();
            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.Unauthorized),
                TokenResponse(),
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"id\": \"retried_user\"}")
                }
            );

            var result = await _api.GetUserID();

            Assert.That(result, Is.EqualTo("retried_user"));
        }

        // ==================== GetTracksInPlaylist ====================

        [Test]
        public async Task GetTracksInPlaylist_ReturnsJObject_When200()
        {
            _handlerMock.SetupResponse(HttpStatusCode.OK,
                new StringContent("{\"items\": [{\"track\": {\"id\": \"track1\"}}], \"next\": null}"));

            var result = await _api.GetTracksInPlaylist("https://api.spotify.com/v1/playlists/p1/tracks");

            Assert.That(result, Is.Not.Null);
            Assert.That(result["items"][0]["track"]["id"].ToString(), Is.EqualTo("track1"));
        }

        [Test]
        public async Task GetTracksInPlaylist_ReturnsNull_When400()
        {
            _handlerMock.SetupResponse(HttpStatusCode.BadRequest,
                new StringContent("{\"error\": \"bad\"}"));

            var result = await _api.GetTracksInPlaylist("https://api.spotify.com/v1/playlists/p1/tracks");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetTracksInPlaylist_ReturnsNull_WhenUrlIsNull()
        {
            var result = await _api.GetTracksInPlaylist(null);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetTracksInPlaylist_RetriesAfterTokenRefresh()
        {
            SetupRefreshTokenDependencies();
            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.Unauthorized),
                TokenResponse(),
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"items\": [], \"next\": null}")
                }
            );

            var result = await _api.GetTracksInPlaylist("https://api.spotify.com/v1/playlists/p1/tracks");

            Assert.That(result, Is.Not.Null);
        }

        // ==================== AddTracksToSQLPlaylist (pure logic) ====================

        [Test]
        public async Task AddTracksToSQLPlaylist_ReturnsTracks_WhenNotInPlaylist()
        {
            var existingTracks = new List<string> { "existing_track" };

            var result = await _api.AddTracksToSQLPlaylist("New Song", "New Artist", "new_track", existingTracks, "playlist_1");

            Assert.That(result.spotifyTracks, Is.Not.Null);
            Assert.That(result.playlistTracks, Is.Not.Null);
            Assert.That(result.spotifyTracks.TrackID, Is.EqualTo("new_track"));
            Assert.That(result.spotifyTracks.TrackName, Is.EqualTo("New Song"));
            Assert.That(result.spotifyTracks.ArtistName, Is.EqualTo("New Artist"));
            Assert.That(result.playlistTracks.PlaylistID, Is.EqualTo("playlist_1"));
            Assert.That(result.playlistTracks.TrackID, Is.EqualTo("new_track"));
        }

        [Test]
        public async Task AddTracksToSQLPlaylist_ReturnsNull_WhenTrackExists()
        {
            var existingTracks = new List<string> { "existing_track" };

            var result = await _api.AddTracksToSQLPlaylist("Existing Song", "Artist", "existing_track", existingTracks, "playlist_1");

            Assert.That(result.spotifyTracks, Is.Null);
            Assert.That(result.playlistTracks, Is.Null);
        }

        [Test]
        public async Task AddTracksToSQLPlaylist_ReturnsTracks_WhenExistingTracksNull()
        {
            var result = await _api.AddTracksToSQLPlaylist("New Song", "New Artist", "new_track", null, "playlist_1");

            Assert.That(result.spotifyTracks, Is.Not.Null);
            Assert.That(result.playlistTracks, Is.Not.Null);
        }

        [Test]
        public async Task AddTracksToSQLPlaylist_ReturnsTracks_WhenExistingTracksEmpty()
        {
            var result = await _api.AddTracksToSQLPlaylist("New Song", "New Artist", "new_track", new List<string>(), "playlist_1");

            Assert.That(result.spotifyTracks, Is.Not.Null);
            Assert.That(result.playlistTracks, Is.Not.Null);
        }

        // ==================== SearchForTracks ====================

        [Test]
        public async Task SearchForTracks_ReturnsTrack_When200()
        {
            _handlerMock.SetupResponse(HttpStatusCode.OK,
                new StringContent("{\"tracks\": {\"items\": [{\"name\": \"Found Song\", \"id\": \"found_id\", \"artists\": [{\"name\": \"Found Artist\"}]}]}}"));

            var result = await _api.SearchForTracks("Found Song", "Found Artist");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.TrackID, Is.EqualTo("found_id"));
            Assert.That(result.TrackName, Is.EqualTo("Found Song"));
        }

        // ==================== DeleteTrackFromPlaylist ====================

        [Test]
        public async Task DeleteTrackFromPlaylist_ReturnsSnapshotId_When200()
        {
            _handlerMock.SetupResponse(HttpStatusCode.OK,
                new StringContent("{\"snapshot_id\": \"deleted_snapshot\"}"));

            var result = await _api.DeleteTrackFromPlaylist("playlist_1", new[] { "track_to_delete" });

            Assert.That(result, Is.EqualTo("deleted_snapshot"));
        }

        [Test]
        public async Task DeleteTrackFromPlaylist_ReturnsNull_WhenTrackIDsEmpty()
        {
            var result = await _api.DeleteTrackFromPlaylist("playlist_1", Array.Empty<string>());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteTrackFromPlaylist_ReturnsNull_WhenMoreThan100()
        {
            var ids = new string[101];
            for (int i = 0; i < 101; i++) ids[i] = $"track_{i}";

            var result = await _api.DeleteTrackFromPlaylist("playlist_1", ids);

            Assert.That(result, Is.Null);
        }

        // ==================== CreatePlaylist ====================

        [Test]
        public async Task CreatePlaylist_ReturnsPlaylist_When200()
        {
            SetupRefreshTokenDependencies();
            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\": \"test_user_001\"}") },
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"snapshot_id\": \"snap_create\", \"id\": \"gap_new_playlist\"}") }
            );

            var result = await _api.CreatePlaylist("Gap Test Playlist");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.PlaylistID, Is.EqualTo("gap_new_playlist"));
            Assert.That(result.Name, Is.EqualTo("Gap Test Playlist"));
            Assert.That(result.SnapshotID, Is.EqualTo("snap_create"));
        }

        // ==================== StorePlaylistToDB ====================

        [Test]
        public async Task StorePlaylistToDB_ReturnsName_When200()
        {
            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"name\": \"Gap Stored Playlist\"}") },
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"snapshot_id\": \"snap_store\"}") }
            );

            var result = await _api.StorePlaylistToDB("gap_store_playlist");

            Assert.That(result, Is.EqualTo("Gap Stored Playlist"));
        }

        [Test]
        public async Task StorePlaylistToDB_ReturnsNull_When400()
        {
            _handlerMock.SetupResponse(HttpStatusCode.BadRequest, new StringContent("{\"error\": \"bad\"}"));

            var result = await _api.StorePlaylistToDB("gap_store_fail");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task StorePlaylistToDB_RetriesAfterTokenRefresh()
        {
            SetupRefreshTokenDependencies();
            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.Unauthorized),
                TokenResponse(),
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"name\": \"Gap Retry Playlist\"}") },
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"snapshot_id\": \"snap_retry\"}") }
            );

            var result = await _api.StorePlaylistToDB("gap_retry_playlist");

            Assert.That(result, Is.EqualTo("Gap Retry Playlist"));
        }

        // ==================== CheckSnapshotIdChangeAsync ====================

        [Test]
        public async Task CheckSnapshotIdChangeAsync_ReturnsTrue_WhenSnapshotChanged()
        {
            await MusicDBApi.PostSpotifyPlaylist(new SpotifyPlaylistsModels
            {
                PlaylistID = "gap_check_changed",
                Name = "Check Test",
                SnapshotID = "old_snapshot"
            });

            _handlerMock.SetupResponse(HttpStatusCode.OK,
                new StringContent("{\"snapshot_id\": \"new_snapshot\"}"));

            var result = await _api.CheckSnapshotIdChangeAsync("gap_check_changed");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CheckSnapshotIdChangeAsync_ReturnsFalse_WhenSnapshotSame()
        {
            await MusicDBApi.PostSpotifyPlaylist(new SpotifyPlaylistsModels
            {
                PlaylistID = "gap_check_same",
                Name = "Check Same",
                SnapshotID = "same_snapshot"
            });

            _handlerMock.SetupResponse(HttpStatusCode.OK,
                new StringContent("{\"snapshot_id\": \"same_snapshot\"}"));

            var result = await _api.CheckSnapshotIdChangeAsync("gap_check_same");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CheckSnapshotIdChangeAsync_ReturnsFalse_WhenNoStoredSnapshot()
        {
            _handlerMock.SetupResponse(HttpStatusCode.OK,
                new StringContent("{\"snapshot_id\": \"any_snapshot\"}"));

            var result = await _api.CheckSnapshotIdChangeAsync("gap_check_nonexistent");

            Assert.That(result, Is.False);
        }

        // ==================== AddTrackToPlaylistchunk ====================

        [Test]
        public async Task AddTrackToPlaylistchunk_ReturnsSnapshot_When200()
        {
            _handlerMock.SetupResponse(HttpStatusCode.OK,
                new StringContent("{\"snapshot_id\": \"chunk_snap\"}"));

            var result = await _api.AddTrackToPlaylistchunk("gap_chunk_playlist", new[] { "gap_chunk_track_1", "gap_chunk_track_2" });

            Assert.That(result, Is.EqualTo("chunk_snap"));
        }

        [Test]
        public async Task AddTrackToPlaylistchunk_ReturnsNull_WhenOver100()
        {
            var ids = new string[101];
            for (int i = 0; i < 101; i++) ids[i] = $"track_{i}";

            var result = await _api.AddTrackToPlaylistchunk("playlist_1", ids);

            Assert.That(result, Is.Null);
        }

        // ==================== AddTrackToPlaylist ====================

        [Test]
        public async Task AddTrackToPlaylist_ReturnsSnapshot_When200()
        {
            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"snapshot_id\": \"add_snap\"}") },
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"snapshot_id\": \"final_snap\"}") }
            );

            var result = await _api.AddTrackToPlaylist("gap_add_playlist", new[] { "gap_add_track_1" });

            Assert.That(result, Is.EqualTo("final_snap"));
        }

        // ==================== StorePlaylistInfoToDBAsync ====================

        [Test]
        public async Task StorePlaylistInfoToDBAsync_ReturnsTracks_When200()
        {
            _handlerMock.SetupResponse(HttpStatusCode.OK,
                new StringContent("{\"items\": [{\"track\": {\"id\": \"gap_info_t1\", \"name\": \"Info Song\", \"artists\": [{\"name\": \"Info Artist\"}]}}], \"next\": null}"));

            var result = await _api.StorePlaylistInfoToDBAsync("gap_info_playlist");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TrackID, Is.EqualTo("gap_info_t1"));
            Assert.That(result[0].TrackName, Is.EqualTo("Info Song"));
            Assert.That(result[0].ArtistName, Is.EqualTo("Info Artist"));
        }

        // ==================== SearchForTracks ====================

        [Test]
        public async Task SearchForTracks_ReturnsNull_WhenApiFails()
        {
            SetupRefreshTokenDependencies();
            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.Unauthorized),
                TokenResponse(),
                new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("") }
            );

            var result = await _api.SearchForTracks("Any Song", "Any Artist");

            Assert.That(result, Is.Null);
        }

    }
}
