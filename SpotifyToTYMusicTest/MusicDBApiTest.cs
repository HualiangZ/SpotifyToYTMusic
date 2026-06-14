using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;

namespace SpotifyToTYMusicTest
{
    [Category("Integration")]
    public class MusicDBApiTest
    {
        private const string TestPlaylistId = "test_sp_playlist_001";
        private const string TestTrackId = "test_track_001";
        private const string TestTrackId2 = "test_track_002";
        private const string TestYTPlaylistId = "test_yt_playlist_001";
        private const string TestYTPlaylistName = "Test YT Playlist";
        private const string TestYTVideoId = "test_video_001";
        private const string TestYTVideoId2 = "test_video_002";
        private const string TestSnapshotId = "snap_001";

        [SetUp]
        public async Task Setup()
        {
            await CleanTestData();
        }

        [TearDown]
        public async Task Teardown()
        {
            await CleanTestData();
        }

        private static async Task CleanTestData()
        {
            await DeletePlaylistSyncByPlaylistIds();
            await MusicDBApi.DeleteSpotifyTrackFromPlaylist(new SpotifyPlaylistTracks
            {
                PlaylistID = TestPlaylistId,
                TrackID = TestTrackId
            });
            await MusicDBApi.DeleteSpotifyTrackFromPlaylist(new SpotifyPlaylistTracks
            {
                PlaylistID = TestPlaylistId,
                TrackID = TestTrackId2
            });
            await MusicDBApi.DeleteYTTrackFromPlaylist(new YouTubePlaylistTracks
            {
                PlaylistID = TestYTPlaylistId,
                TrackID = TestYTVideoId
            });
            await MusicDBApi.DeleteYTTrackFromPlaylist(new YouTubePlaylistTracks
            {
                PlaylistID = TestYTPlaylistId,
                TrackID = TestYTVideoId2
            });
            await MusicDBApi.DeleteSpotifyPlaylist(TestPlaylistId);
            await MusicDBApi.DeleteSpotifyTrack(TestTrackId);
            await MusicDBApi.DeleteSpotifyTrack(TestTrackId2);
            await MusicDBApi.DeleteYouTubeTrack(TestYTVideoId);
            await MusicDBApi.DeleteYouTubeTrack(TestYTVideoId2);
            await MusicDBApi.DeleteYouTubePlaylists(new YoutubePlaylistsModel
            {
                PlaylistID = TestYTPlaylistId,
                Name = TestYTPlaylistName
            });
            await CleanGapData();
        }

        private static async Task DeletePlaylistSyncByPlaylistIds()
        {
            try
            {
                using var cnn = new System.Data.SQLite.SQLiteConnection(
                    "Data Source=./MusicDB.db;foreign keys=true;");
                await cnn.OpenAsync();
                using var cmd = cnn.CreateCommand();
                cmd.CommandText = "DELETE FROM PlaylistSync WHERE SpotifyPlaylistID = @sp OR YTPlaylistID = @yt";
                cmd.Parameters.AddWithValue("@sp", TestPlaylistId);
                cmd.Parameters.AddWithValue("@yt", TestYTPlaylistId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }

        private static async Task InsertSpotifyTrack(string id, string name, string artist)
        {
            var track = new SpotifyTracks { TrackID = id, TrackName = name, ArtistName = artist };
            await MusicDBApi.PostSpotifyTrack(track);
        }

        private static async Task InsertSpotifyPlaylist()
        {
            var playlist = new SpotifyPlaylistsModels
            {
                PlaylistID = TestPlaylistId,
                Name = "Test Playlist",
                SnapshotID = TestSnapshotId
            };
            await MusicDBApi.PostSpotifyPlaylist(playlist);
        }

        private static async Task InsertYTPlaylist()
        {
            var playlist = new YoutubePlaylistsModel
            {
                PlaylistID = TestYTPlaylistId,
                Name = TestYTPlaylistName
            };
            await MusicDBApi.PostYouTubePlaylists(playlist);
        }

        private static async Task InsertYTPlaylistTrack()
        {
            var pt = new YouTubePlaylistTracks
            {
                PlaylistID = TestYTPlaylistId,
                TrackID = TestYTVideoId,
                ID = "yt_item_001"
            };
            await MusicDBApi.PostYTTrackToPlaylist(pt);
        }

        // ======== SpotifyTracks CRUD ========

        [Test]
        public async Task PostAndGetSpotifyTrack_Succeeds()
        {
            await InsertSpotifyTrack(TestTrackId, "Test Song", "Test Artist");

            var getResult = await MusicDBApi.GetSpotifyTrack(TestTrackId);
            Assert.That(getResult.Err, Is.Null);
            Assert.That(getResult.Track.TrackID, Is.EqualTo(TestTrackId));
            Assert.That(getResult.Track.TrackName, Is.EqualTo("Test Song"));
            Assert.That(getResult.Track.ArtistName, Is.EqualTo("Test Artist"));
        }

        [Test]
        public async Task PostAndDeleteSpotifyTrack_Succeeds()
        {
            await InsertSpotifyTrack(TestTrackId, "Delete Me", "Artist");

            var deleteResult = await MusicDBApi.DeleteSpotifyTrack(TestTrackId);
            Assert.That(deleteResult.Success, Is.True);
        }

        [Test]
        public async Task PostSpotifyTrack_DuplicateInsert_Fails()
        {
            await InsertSpotifyTrack(TestTrackId, "Original", "Artist");

            var duplicate = new SpotifyTracks { TrackID = TestTrackId, TrackName = "Duplicate", ArtistName = "Artist" };
            var result = await MusicDBApi.PostSpotifyTrack(duplicate);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Err, Does.Contain("UNIQUE").Or.Contain("PRIMARY KEY"));
        }

        [Test]
        public async Task PostSpotifyTracks_Bulk_Succeeds()
        {
            var tracks = new List<SpotifyTracks>
            {
                new() { TrackID = TestTrackId, TrackName = "Song A", ArtistName = "Artist A" },
                new() { TrackID = TestTrackId2, TrackName = "Song B", ArtistName = "Artist B" }
            };

            var result = await MusicDBApi.PostSpotifyTracks(tracks);
            Assert.That(result.Success, Is.True);

            var getA = await MusicDBApi.GetSpotifyTrack(TestTrackId);
            Assert.That(getA.Track, Is.Not.Null);
            var getB = await MusicDBApi.GetSpotifyTrack(TestTrackId2);
            Assert.That(getB.Track, Is.Not.Null);
        }

        // ======== SpotifyPlaylists CRUD ========

        [Test]
        public async Task PostAndGetSpotifyPlaylist_Succeeds()
        {
            await InsertSpotifyPlaylist();

            var getResult = await MusicDBApi.GetOneSportifyPlaylists(TestPlaylistId);
            Assert.That(getResult.Err, Is.Null);
            Assert.That(getResult.Playlist.Name, Is.EqualTo("Test Playlist"));
            Assert.That(getResult.Playlist.SnapshotID, Is.EqualTo(TestSnapshotId));
        }

        [Test]
        public async Task UpdateSpotifyPlaylistSnapshotID_Succeeds()
        {
            await InsertSpotifyPlaylist();

            var updateResult = await MusicDBApi.UpdateSpotifyPlaylistSnapshotID(TestPlaylistId, "new_snap");
            Assert.That(updateResult.Success, Is.True);

            var getResult = await MusicDBApi.GetOneSportifyPlaylists(TestPlaylistId);
            Assert.That(getResult.Playlist.SnapshotID, Is.EqualTo("new_snap"));
        }

        [Test]
        public async Task GetAllSportifyPlaylists_ReturnsPlaylists()
        {
            await InsertSpotifyPlaylist();

            var result = await MusicDBApi.GetAllSportifyPlaylists();
            Assert.That(result.Err, Is.Null);
            Assert.That(result.Playlists.Any(p => p.PlaylistID == TestPlaylistId), Is.True);
        }

        // ======== SpotifyPlaylistTracks CRUD ========

        [Test]
        public async Task PostSpotifyTrackToPlaylist_Succeeds()
        {
            await InsertSpotifyPlaylist();
            await InsertSpotifyTrack(TestTrackId, "N", "A");

            var pt = new SpotifyPlaylistTracks { PlaylistID = TestPlaylistId, TrackID = TestTrackId };
            var result = await MusicDBApi.PostSpotifyTrackToPlaylist(pt);
            Assert.That(result.Success, Is.True);

            var getResult = await MusicDBApi.GetAllSpotifyTrackInPlaylist(TestPlaylistId);
            Assert.That(getResult.Tracks, Does.Contain(TestTrackId));
        }

        [Test]
        public async Task DeleteSpotifyTrackFromPlaylist_Succeeds()
        {
            await InsertSpotifyPlaylist();
            await InsertSpotifyTrack(TestTrackId, "N", "A");
            var pt = new SpotifyPlaylistTracks { PlaylistID = TestPlaylistId, TrackID = TestTrackId };
            await MusicDBApi.PostSpotifyTrackToPlaylist(pt);

            var deleteResult = await MusicDBApi.DeleteSpotifyTrackFromPlaylist(pt);
            Assert.That(deleteResult.Success, Is.True);
        }

        [Test]
        public async Task PostSpotifyTracksToPlaylist_Bulk_Succeeds()
        {
            await InsertSpotifyPlaylist();
            await InsertSpotifyTrack(TestTrackId, "N", "A");
            await InsertSpotifyTrack(TestTrackId2, "M", "B");

            var playlistTracks = new List<SpotifyPlaylistTracks>
            {
                new() { PlaylistID = TestPlaylistId, TrackID = TestTrackId },
                new() { PlaylistID = TestPlaylistId, TrackID = TestTrackId2 }
            };
            var result = await MusicDBApi.PostSpotifyTracksToPlaylist(playlistTracks);
            Assert.That(result.Success, Is.True);
        }

        [Test]
        public async Task GetAllSpotifyTrackInPlaylist_ReturnsEmpty_WhenNoTracks()
        {
            await InsertSpotifyPlaylist();
            var result = await MusicDBApi.GetAllSpotifyTrackInPlaylist(TestPlaylistId);
            Assert.That(result.Tracks, Is.Empty);
        }

        // ======== YouTubeTracks CRUD ========

        [Test]
        public async Task PostAndGetYouTubeTrack_Succeeds()
        {
            var track = new YouTubeTracks { TrackID = TestYTVideoId, TrackName = "YT Song", ArtistName = "YT Artist" };
            var result = await MusicDBApi.PostYouTubeTrack(track);
            Assert.That(result.Success, Is.True);

            var getResult = await MusicDBApi.GetYouTubeTrack(TestYTVideoId);
            Assert.That(getResult.Err, Is.Null);
            Assert.That(getResult.Track.TrackName, Is.EqualTo("YT Song"));
        }

        [Test]
        public async Task PostYouTubeTrack_Duplicate_Ignored()
        {
            var track = new YouTubeTracks { TrackID = TestYTVideoId, TrackName = "First", ArtistName = "A" };
            await MusicDBApi.PostYouTubeTrack(track);

            var dup = new YouTubeTracks { TrackID = TestYTVideoId, TrackName = "Second", ArtistName = "B" };
            var result = await MusicDBApi.PostYouTubeTrack(dup);
            Assert.That(result.Success, Is.True);

            var getResult = await MusicDBApi.GetYouTubeTrack(TestYTVideoId);
            Assert.That(getResult.Track.TrackName, Is.EqualTo("First"));
        }

        [Test]
        public async Task PostAndDeleteYouTubeTrack_Succeeds()
        {
            var track = new YouTubeTracks { TrackID = TestYTVideoId, TrackName = "Del", ArtistName = "A" };
            await MusicDBApi.PostYouTubeTrack(track);

            var deleteResult = await MusicDBApi.DeleteYouTubeTrack(TestYTVideoId);
            Assert.That(deleteResult.Success, Is.True);
        }

        [Test]
        public async Task PostYouTubeTrack_Bulk_Succeeds()
        {
            var tracks = new List<YouTubeTracks>
            {
                new() { TrackID = TestYTVideoId, TrackName = "N", ArtistName = "A" },
                new() { TrackID = TestYTVideoId2, TrackName = "M", ArtistName = "B" }
            };
            var result = await MusicDBApi.PostYouTubeTrack(tracks);
            Assert.That(result.Success, Is.True);
        }

        // ======== YouTubePlaylists CRUD ========

        [Test]
        public async Task PostAndGetYouTubePlaylist_Succeeds()
        {
            await InsertYTPlaylist();

            var getResult = await MusicDBApi.GetOneYTPlaylist(TestYTPlaylistId);
            Assert.That(getResult.Err, Is.Null);
            Assert.That(getResult.Playlist.Name, Is.EqualTo(TestYTPlaylistName));
        }

        [Test]
        public async Task PostAndDeleteYouTubePlaylist_Succeeds()
        {
            await InsertYTPlaylist();

            var deleteResult = await MusicDBApi.DeleteYouTubePlaylists(
                new YoutubePlaylistsModel { PlaylistID = TestYTPlaylistId, Name = TestYTPlaylistName });
            Assert.That(deleteResult.Success, Is.True);
        }

        // ======== YouTubePlaylistTracks CRUD ========

        [Test]
        public async Task PostYTPlaylistTrack_Succeeds()
        {
            await InsertYTPlaylist();
            var track = new YouTubeTracks { TrackID = TestYTVideoId, TrackName = "N", ArtistName = "A" };
            await MusicDBApi.PostYouTubeTrack(track);

            var pt = new YouTubePlaylistTracks { PlaylistID = TestYTPlaylistId, TrackID = TestYTVideoId, ID = "yt_item_001" };
            var result = await MusicDBApi.PostYTTrackToPlaylist(pt);
            Assert.That(result.success, Is.True);

            var getResult = await MusicDBApi.GetAllYTTracksfromPlaylist(TestYTPlaylistId);
            Assert.That(getResult.Tracks.Any(t => t.TrackID == TestYTVideoId), Is.True);
        }

        [Test]
        public async Task DeleteYTTrackFromPlaylist_Succeeds()
        {
            await InsertYTPlaylist();
            var track = new YouTubeTracks { TrackID = TestYTVideoId, TrackName = "N", ArtistName = "A" };
            await MusicDBApi.PostYouTubeTrack(track);
            var pt = new YouTubePlaylistTracks { PlaylistID = TestYTPlaylistId, TrackID = TestYTVideoId, ID = "yt_item_002" };
            await MusicDBApi.PostYTTrackToPlaylist(pt);

            var deleteResult = await MusicDBApi.DeleteYTTrackFromPlaylist(pt);
            Assert.That(deleteResult.success, Is.True);
        }

        [Test]
        public async Task PostYTPlaylistTracks_Bulk_Succeeds()
        {
            await InsertYTPlaylist();
            var tracks = new List<YouTubeTracks>
            {
                new() { TrackID = TestYTVideoId, TrackName = "N", ArtistName = "A" },
                new() { TrackID = TestYTVideoId2, TrackName = "M", ArtistName = "B" }
            };
            await MusicDBApi.PostYouTubeTrack(tracks);

            var playlistTracks = new List<YouTubePlaylistTracks>
            {
                new() { PlaylistID = TestYTPlaylistId, TrackID = TestYTVideoId, ID = "item1" },
                new() { PlaylistID = TestYTPlaylistId, TrackID = TestYTVideoId2, ID = "item2" }
            };
            var result = await MusicDBApi.PostYTTrackToPlaylist(playlistTracks);
            Assert.That(result.success, Is.True);
        }

        [Test]
        public async Task GetTrackFromYTPlaylist_Succeeds()
        {
            await InsertYTPlaylist();
            var track = new YouTubeTracks { TrackID = TestYTVideoId, TrackName = "N", ArtistName = "A" };
            await MusicDBApi.PostYouTubeTrack(track);
            var pt = new YouTubePlaylistTracks { PlaylistID = TestYTPlaylistId, TrackID = TestYTVideoId, ID = "item_specific" };
            await MusicDBApi.PostYTTrackToPlaylist(pt);

            var result = await MusicDBApi.GetTrackFromTYPlaylist(TestYTPlaylistId, TestYTVideoId);
            Assert.That(result.Err, Is.Null);
            Assert.That(result.Track.PlaylistID, Is.EqualTo(TestYTPlaylistId));
            Assert.That(result.Track.TrackID, Is.EqualTo(TestYTVideoId));
        }

        // ======== PlaylistSync CRUD ========

        [Test]
        public async Task PostAndGetPlaylistSync_Succeeds()
        {
            await InsertSpotifyPlaylist();
            await InsertYTPlaylist();
            await MusicDBApi.PostPlaylistSync(TestYTPlaylistId, TestPlaylistId, Direction.Spotify, Sync.On);

            var spotifyResult = await MusicDBApi.GetSyncedPlaylistWithSpotify(TestPlaylistId);
            Assert.That(spotifyResult.Err, Is.Null);
            Assert.That(spotifyResult.PlaylistId, Is.EqualTo(TestYTPlaylistId));

            var ytResult = await MusicDBApi.GetSyncedPlaylistWithYouTube(TestYTPlaylistId);
            Assert.That(ytResult.Err, Is.Null);
            Assert.That(ytResult.PlaylistId, Is.EqualTo(TestPlaylistId));
        }

        [Test]
        public async Task GetAllSyncedPlaylists_ReturnsAll()
        {
            await InsertSpotifyPlaylist();
            await InsertYTPlaylist();
            await MusicDBApi.PostPlaylistSync(TestYTPlaylistId, TestPlaylistId, Direction.Spotify, Sync.On);

            var result = await MusicDBApi.GetAllSyncedPlaylists();
            Assert.That(result.Err, Is.Null);
            Assert.That(result.PlaylistSync.Any(p => p.SpotifyPlaylistID == TestPlaylistId), Is.True);
        }

        [Test]
        public async Task GetAllPlaylistSync_FiltersByDirection()
        {
            await InsertSpotifyPlaylist();
            await InsertYTPlaylist();
            await MusicDBApi.PostPlaylistSync(TestYTPlaylistId, TestPlaylistId, Direction.Spotify, Sync.On);

            var spotifyList = await MusicDBApi.GetAllPlaylistSync(Direction.Spotify);
            Assert.That(spotifyList.Err, Is.Null);
            Assert.That(spotifyList.PlaylistSync.Any(p => p.SpotifyPlaylistID == TestPlaylistId), Is.True);

            var ytList = await MusicDBApi.GetAllPlaylistSync(Direction.YouTube);
            Assert.That(ytList.Err, Is.Null);
            Assert.That(ytList.PlaylistSync.Any(p => p.SpotifyPlaylistID == TestPlaylistId), Is.False);
        }

        [Test]
        public async Task GetUnsyncedTrackToAddYouTube_ReturnsNewTracks()
        {
            await InsertSpotifyPlaylist();
            await InsertYTPlaylist();
            await MusicDBApi.PostPlaylistSync(TestYTPlaylistId, TestPlaylistId, Direction.Spotify, Sync.On);

            await InsertSpotifyTrack(TestTrackId, "Common Song", "Common Artist");
            var pt = new SpotifyPlaylistTracks { PlaylistID = TestPlaylistId, TrackID = TestTrackId };
            await MusicDBApi.PostSpotifyTrackToPlaylist(pt);

            var ytTrack = new YouTubeTracks { TrackID = TestYTVideoId, TrackName = "Common Song", ArtistName = "Common Artist" };
            await MusicDBApi.PostYouTubeTrack(ytTrack);

            var result = await MusicDBApi.GetUnsyncedTrackToAddYouTube(TestPlaylistId);
            Assert.That(result.Err, Is.Null);
            Assert.That(result.Tracks.Any(t => t.TrackID == TestYTVideoId), Is.True);
        }

        [Test]
        public async Task GetUnsyncedTrackToAddYouTube_ReturnsEmpty_WhenAllSynced()
        {
            await InsertSpotifyPlaylist();
            await InsertYTPlaylist();
            await MusicDBApi.PostPlaylistSync(TestYTPlaylistId, TestPlaylistId, Direction.Spotify, Sync.On);

            await InsertSpotifyTrack(TestTrackId, "Song", "Artist");
            var pt = new SpotifyPlaylistTracks { PlaylistID = TestPlaylistId, TrackID = TestTrackId };
            await MusicDBApi.PostSpotifyTrackToPlaylist(pt);

            var ytTrack = new YouTubeTracks { TrackID = TestYTVideoId, TrackName = "Song", ArtistName = "Artist" };
            await MusicDBApi.PostYouTubeTrack(ytTrack);

            var ytPt = new YouTubePlaylistTracks { PlaylistID = TestYTPlaylistId, TrackID = TestYTVideoId, ID = "synced_item" };
            await MusicDBApi.PostYTTrackToPlaylist(ytPt);

            var result = await MusicDBApi.GetUnsyncedTrackToAddYouTube(TestPlaylistId);
            Assert.That(result.Tracks, Is.Empty);
        }

        // ======== Previously untested methods ========

        private const string GapSpPlaylistId = "gap_sp_pl_001";
        private const string GapYTPlaylistId = "gap_yt_pl_001";
        private const string GapTrackId = "gap_tr_001";
        private const string GapTrackId2 = "gap_tr_002";
        private const string GapYTVideoId = "gap_vid_001";
        private const string GapYTVideoId2 = "gap_vid_002";

        private static async Task CleanGapData()
        {
            await DeleteGapPlaylistSync();
            await MusicDBApi.DeleteSpotifyTrackFromPlaylist(new SpotifyPlaylistTracks { PlaylistID = GapSpPlaylistId, TrackID = GapTrackId });
            await MusicDBApi.DeleteSpotifyTrackFromPlaylist(new SpotifyPlaylistTracks { PlaylistID = GapSpPlaylistId, TrackID = GapTrackId2 });
            await MusicDBApi.DeleteYTTrackFromPlaylist(new YouTubePlaylistTracks { PlaylistID = GapYTPlaylistId, TrackID = GapYTVideoId });
            await MusicDBApi.DeleteYTTrackFromPlaylist(new YouTubePlaylistTracks { PlaylistID = GapYTPlaylistId, TrackID = GapYTVideoId2 });
            await MusicDBApi.DeleteSpotifyPlaylist(GapSpPlaylistId);
            await MusicDBApi.DeleteSpotifyTrack(GapTrackId);
            await MusicDBApi.DeleteSpotifyTrack(GapTrackId2);
            await MusicDBApi.DeleteYouTubeTrack(GapYTVideoId);
            await MusicDBApi.DeleteYouTubeTrack(GapYTVideoId2);
            await MusicDBApi.DeleteYouTubePlaylists(new YoutubePlaylistsModel { PlaylistID = GapYTPlaylistId, Name = "Gap Test" });
        }

        private static async Task DeleteGapPlaylistSync()
        {
            try
            {
                using var cnn = new System.Data.SQLite.SQLiteConnection(
                    "Data Source=./MusicDB.db;foreign keys=true;");
                await cnn.OpenAsync();
                using var cmd = cnn.CreateCommand();
                cmd.CommandText = "DELETE FROM PlaylistSync WHERE SpotifyPlaylistID = @sp OR YTPlaylistID = @yt";
                cmd.Parameters.AddWithValue("@sp", GapSpPlaylistId);
                cmd.Parameters.AddWithValue("@yt", GapYTPlaylistId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }

        [Test]
        public async Task GetSpotifyTrackByNameAndArtist_Succeeds()
        {
            await MusicDBApi.PostSpotifyTrack(new SpotifyTracks { TrackID = GapTrackId, TrackName = "Gap Song", ArtistName = "Gap Artist" });

            var result = await MusicDBApi.GetSpotifyTrack("Gap Song", "Gap Artist");

            Assert.That(result.Err, Is.Null);
            Assert.That(result.Track.TrackID, Is.EqualTo(GapTrackId));
            Assert.That(result.Track.TrackName, Is.EqualTo("Gap Song"));
            Assert.That(result.Track.ArtistName, Is.EqualTo("Gap Artist"));
        }

        [Test]
        public async Task GetUnsyncedTracksToRemoveYouTube_ReturnsTracks()
        {
            await MusicDBApi.PostSpotifyPlaylist(new SpotifyPlaylistsModels { PlaylistID = GapSpPlaylistId, Name = "Gap SP", SnapshotID = "snap" });
            await MusicDBApi.PostYouTubePlaylists(new YoutubePlaylistsModel { PlaylistID = GapYTPlaylistId, Name = "Gap YT" });
            await MusicDBApi.PostPlaylistSync(GapYTPlaylistId, GapSpPlaylistId, Direction.YouTube, Sync.On);

            await MusicDBApi.PostSpotifyTrack(new SpotifyTracks { TrackID = GapTrackId, TrackName = "Common Song", ArtistName = "Common Artist" });
            await MusicDBApi.PostSpotifyTrackToPlaylist(new SpotifyPlaylistTracks { PlaylistID = GapSpPlaylistId, TrackID = GapTrackId });

            await MusicDBApi.PostYouTubeTrack(new YouTubeTracks { TrackID = GapYTVideoId, TrackName = "Common Song", ArtistName = "Common Artist" });
            await MusicDBApi.PostYTTrackToPlaylist(new YouTubePlaylistTracks { PlaylistID = GapYTPlaylistId, TrackID = GapYTVideoId, ID = "yt_item_common" });

            await MusicDBApi.PostYouTubeTrack(new YouTubeTracks { TrackID = GapYTVideoId2, TrackName = "Only YT Song", ArtistName = "Only YT Artist" });
            await MusicDBApi.PostYTTrackToPlaylist(new YouTubePlaylistTracks { PlaylistID = GapYTPlaylistId, TrackID = GapYTVideoId2, ID = "yt_item_orphan" });

            var result = await MusicDBApi.GetUnsyncedTracksToRemoveYouTube(GapYTPlaylistId);

            Assert.That(result.Err, Is.Null);
            Assert.That(result.Tracks.Any(t => t.TrackID == GapYTVideoId2), Is.True);
            Assert.That(result.Tracks.Any(t => t.TrackID == GapYTVideoId), Is.False);
        }

        [Test]
        public async Task GetUnsyncedTrackToAddSpotify_ReturnsTracks()
        {
            await MusicDBApi.PostSpotifyPlaylist(new SpotifyPlaylistsModels { PlaylistID = GapSpPlaylistId, Name = "Gap SP", SnapshotID = "snap" });
            await MusicDBApi.PostYouTubePlaylists(new YoutubePlaylistsModel { PlaylistID = GapYTPlaylistId, Name = "Gap YT" });
            await MusicDBApi.PostPlaylistSync(GapYTPlaylistId, GapSpPlaylistId, Direction.YouTube, Sync.On);

            await MusicDBApi.PostYouTubeTrack(new YouTubeTracks { TrackID = GapYTVideoId, TrackName = "Synced YT Song", ArtistName = "Artist A" });
            await MusicDBApi.PostYTTrackToPlaylist(new YouTubePlaylistTracks { PlaylistID = GapYTPlaylistId, TrackID = GapYTVideoId, ID = "yt_synced" });
            await MusicDBApi.PostSpotifyTrack(new SpotifyTracks { TrackID = GapTrackId, TrackName = "Synced YT Song", ArtistName = "Artist A" });
            await MusicDBApi.PostSpotifyTrackToPlaylist(new SpotifyPlaylistTracks { PlaylistID = GapSpPlaylistId, TrackID = GapTrackId });

            await MusicDBApi.PostYouTubeTrack(new YouTubeTracks { TrackID = GapYTVideoId2, TrackName = "Unsynced YT Song", ArtistName = "Artist B" });
            await MusicDBApi.PostYTTrackToPlaylist(new YouTubePlaylistTracks { PlaylistID = GapYTPlaylistId, TrackID = GapYTVideoId2, ID = "yt_unsynced" });

            var result = await MusicDBApi.GetUnsyncedTrackToAddSpotify(GapYTPlaylistId);

            Assert.That(result.Err, Is.Null);
            Assert.That(result.Tracks.Any(t => t.TrackID == GapYTVideoId2), Is.True);
            Assert.That(result.Tracks.Any(t => t.TrackID == GapYTVideoId), Is.False);
        }

        [Test]
        public async Task GetUnsyncedTrackToRemoveSpotify_ReturnsTracks()
        {
            await MusicDBApi.PostSpotifyPlaylist(new SpotifyPlaylistsModels { PlaylistID = GapSpPlaylistId, Name = "Gap SP", SnapshotID = "snap" });
            await MusicDBApi.PostYouTubePlaylists(new YoutubePlaylistsModel { PlaylistID = GapYTPlaylistId, Name = "Gap YT" });
            await MusicDBApi.PostPlaylistSync(GapYTPlaylistId, GapSpPlaylistId, Direction.YouTube, Sync.On);

            await MusicDBApi.PostSpotifyTrack(new SpotifyTracks { TrackID = GapTrackId, TrackName = "Keep Me", ArtistName = "Artist" });
            await MusicDBApi.PostSpotifyTrackToPlaylist(new SpotifyPlaylistTracks { PlaylistID = GapSpPlaylistId, TrackID = GapTrackId });
            await MusicDBApi.PostYouTubeTrack(new YouTubeTracks { TrackID = GapYTVideoId, TrackName = "Keep Me", ArtistName = "Artist" });
            await MusicDBApi.PostYTTrackToPlaylist(new YouTubePlaylistTracks { PlaylistID = GapYTPlaylistId, TrackID = GapYTVideoId, ID = "yt_keep" });

            await MusicDBApi.PostSpotifyTrack(new SpotifyTracks { TrackID = GapTrackId2, TrackName = "Remove Me", ArtistName = "Artist" });
            await MusicDBApi.PostSpotifyTrackToPlaylist(new SpotifyPlaylistTracks { PlaylistID = GapSpPlaylistId, TrackID = GapTrackId2 });

            var result = await MusicDBApi.GetUnsyncedTrackToRemoveSpotify(GapYTPlaylistId);

            Assert.That(result.Err, Is.Null);
            Assert.That(result.Tracks.Any(t => t.TrackID == GapTrackId2), Is.True);
            Assert.That(result.Tracks.Any(t => t.TrackID == GapTrackId), Is.False);
        }
    }
}
