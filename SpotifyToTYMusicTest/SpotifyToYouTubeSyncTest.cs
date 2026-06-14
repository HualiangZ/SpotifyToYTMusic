using Moq;
using Spotify_to_YTMusic.Components;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using Spotify_to_YTMusic.Interfaces;

namespace SpotifyToTYMusicTest
{
    [Category("Integration")]
    public class SpotifyToYouTubeSyncTest
    {
        private const string TestSpotifyPlaylistId = "sync_sp_001";
        private const string TestYTPlaylistId = "sync_yt_001";
        private const string TestSpotifyTrackId = "sync_track_sp_001";
        private const string TestYTVideoId = "sync_track_yt_001";
        private const string TestPlaylistName = "Sync Test Playlist";

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
                PlaylistID = TestSpotifyPlaylistId,
                TrackID = TestSpotifyTrackId
            });
            await MusicDBApi.DeleteYTTrackFromPlaylist(new YouTubePlaylistTracks
            {
                PlaylistID = TestYTPlaylistId,
                TrackID = TestYTVideoId
            });
            await MusicDBApi.DeleteSpotifyPlaylist(TestSpotifyPlaylistId);
            await MusicDBApi.DeleteYouTubePlaylists(new YoutubePlaylistsModel
            {
                PlaylistID = TestYTPlaylistId,
                Name = TestPlaylistName
            });
            await MusicDBApi.DeleteSpotifyTrack(TestSpotifyTrackId);
            await MusicDBApi.DeleteYouTubeTrack(TestYTVideoId);
        }

        private static async Task DeletePlaylistSyncByPlaylistIds()
        {
            try
            {
                using var cnn = new System.Data.SQLite.SQLiteConnection(
                    "Data Source=./MusicDB.db;foreign keys=true;");
                await cnn.OpenAsync();
                using var cmd = cnn.CreateCommand();
                cmd.CommandText = "DELETE FROM PlaylistSync";
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }

        [Test]
        public void Constructor_AcceptsApiInstances()
        {
            var spotifyApi = SpotifyApi.Instance();
            var youtubeApi = YoutubeApi.Instance();
            var sync = new SpotifyToYouTubeSync(youtubeApi, spotifyApi);

            Assert.That(sync, Is.Not.Null);
        }

        [Test]
        public void Constructor_ImplementsInterface()
        {
            var spotifyApi = SpotifyApi.Instance();
            var youtubeApi = YoutubeApi.Instance();
            var sync = new SpotifyToYouTubeSync(youtubeApi, spotifyApi);

            Assert.That(sync, Is.InstanceOf<ISpotifyToYouTubeSync>());
        }

        [Test]
        public async Task DatabaseRoundTrip_SpotifyPlaylist_CRUD()
        {
            var playlist = new SpotifyPlaylistsModels
            {
                PlaylistID = TestSpotifyPlaylistId,
                Name = TestPlaylistName,
                SnapshotID = "snap_roundtrip"
            };
            var insertResult = await MusicDBApi.PostSpotifyPlaylist(playlist);
            Assert.That(insertResult.Success, Is.True, $"Insert playlist failed: {insertResult.Err}");

            var getPlaylist = await MusicDBApi.GetOneSportifyPlaylists(TestSpotifyPlaylistId);
            Assert.That(getPlaylist.Playlist.Name, Is.EqualTo(TestPlaylistName));
        }

        [Test]
        public async Task DatabaseRoundTrip_YouTubePlaylist_CRUD()
        {
            var ytPlaylist = new YoutubePlaylistsModel
            {
                PlaylistID = TestYTPlaylistId,
                Name = TestPlaylistName
            };
            var insertResult = await MusicDBApi.PostYouTubePlaylists(ytPlaylist);
            Assert.That(insertResult.Success, Is.True, $"Insert YT playlist failed: {insertResult.Err}");

            var getYTPlaylist = await MusicDBApi.GetOneYTPlaylist(TestYTPlaylistId);
            Assert.That(getYTPlaylist.Playlist.Name, Is.EqualTo(TestPlaylistName));
        }

        [Test]
        public async Task DatabaseRoundTrip_PlaylistSync_CRUD()
        {
            var spotifyPlaylist = new SpotifyPlaylistsModels
            {
                PlaylistID = TestSpotifyPlaylistId,
                Name = TestPlaylistName,
                SnapshotID = "snap"
            };
            await MusicDBApi.PostSpotifyPlaylist(spotifyPlaylist);

            var ytPlaylist = new YoutubePlaylistsModel
            {
                PlaylistID = TestYTPlaylistId,
                Name = TestPlaylistName
            };
            await MusicDBApi.PostYouTubePlaylists(ytPlaylist);

            await MusicDBApi.PostPlaylistSync(TestYTPlaylistId, TestSpotifyPlaylistId, Direction.Spotify, Sync.On);

            var syncResult = await MusicDBApi.GetSyncedPlaylistWithSpotify(TestSpotifyPlaylistId);
            Assert.That(syncResult.PlaylistId, Is.EqualTo(TestYTPlaylistId));

            var ytResult = await MusicDBApi.GetSyncedPlaylistWithYouTube(TestYTPlaylistId);
            Assert.That(ytResult.PlaylistId, Is.EqualTo(TestSpotifyPlaylistId));
        }

        [Test]
        public async Task DatabaseRoundTrip_UpdateSpotifyPlaylistSnapshotID()
        {
            var playlist = new SpotifyPlaylistsModels
            {
                PlaylistID = TestSpotifyPlaylistId,
                Name = "Snapshot Test",
                SnapshotID = "old_snapshot"
            };
            await MusicDBApi.PostSpotifyPlaylist(playlist);

            await MusicDBApi.UpdateSpotifyPlaylistSnapshotID(TestSpotifyPlaylistId, "new_snapshot");

            var getResult = await MusicDBApi.GetOneSportifyPlaylists(TestSpotifyPlaylistId);
            Assert.That(getResult.Playlist.SnapshotID, Is.EqualTo("new_snapshot"));
        }

        [Test]
        public async Task GetAllPlaylistSync_FiltersCorrectly()
        {
            var spotifyPlaylist = new SpotifyPlaylistsModels
            {
                PlaylistID = TestSpotifyPlaylistId,
                Name = TestPlaylistName,
                SnapshotID = "snap"
            };
            await MusicDBApi.PostSpotifyPlaylist(spotifyPlaylist);

            var ytPlaylist = new YoutubePlaylistsModel
            {
                PlaylistID = TestYTPlaylistId,
                Name = TestPlaylistName
            };
            await MusicDBApi.PostYouTubePlaylists(ytPlaylist);

            await MusicDBApi.PostPlaylistSync(TestYTPlaylistId, TestSpotifyPlaylistId, Direction.Spotify, Sync.On);

            var spotifyDir = await MusicDBApi.GetAllPlaylistSync(Direction.Spotify);
            Assert.That(spotifyDir.PlaylistSync.Any(p => p.SpotifyPlaylistID == TestSpotifyPlaylistId), Is.True);

            var allSynced = await MusicDBApi.GetAllSyncedPlaylists();
            Assert.That(allSynced.PlaylistSync.Any(p => p.SpotifyPlaylistID == TestSpotifyPlaylistId), Is.True);
        }

        [Test]
        public async Task UpdateYTPlaylist_DoesNothing_WhenNoSyncedPlaylists()
        {
            var spotifyApi = SpotifyApi.Instance();
            var youtubeApi = YoutubeApi.Instance();
            var sync = new SpotifyToYouTubeSync(youtubeApi, spotifyApi);

            await sync.UpdateYTPlaylist();

            var allSynced = await MusicDBApi.GetAllSyncedPlaylists();
            Assert.That(allSynced.PlaylistSync.Any(p => p.SpotifyPlaylistID == TestSpotifyPlaylistId), Is.False);
        }

        [Test]
        public async Task UpdateSpotifyPlaylist_DoesNothing_WhenNoSyncedPlaylists()
        {
            var spotifyApi = SpotifyApi.Instance();
            var youtubeApi = YoutubeApi.Instance();
            var sync = new SpotifyToYouTubeSync(youtubeApi, spotifyApi);

            await sync.UpdateSpotifyPlaylist();

            var allSynced = await MusicDBApi.GetAllSyncedPlaylists();
            Assert.That(allSynced.PlaylistSync.Any(p => p.YTPlaylistID == TestYTPlaylistId), Is.False);
        }

        // ==================== Moq-based sync logic tests ====================

        private const string MoqSpId = "moq_ss_sp_001";
        private const string MoqYtId = "moq_ss_yt_001";
        private const string MoqTrackId = "moq_ss_st_001";
        private const string MoqYtTrackId = "moq_ss_yt_tr_001";
        private const string MoqYtRemoveId = "moq_ss_yt_rm_001";
        private const string MoqTrackName = "Moq Song A";
        private const string MoqArtistName = "Moq Artist A";

        private static async Task CleanMoqData()
        {
            await MusicDBApi.DeleteSpotifyTrackFromPlaylist(new SpotifyPlaylistTracks { PlaylistID = MoqSpId, TrackID = MoqTrackId });
            await MusicDBApi.DeleteYTTrackFromPlaylist(new YouTubePlaylistTracks { PlaylistID = MoqYtId, TrackID = MoqYtTrackId });
            await MusicDBApi.DeleteYTTrackFromPlaylist(new YouTubePlaylistTracks { PlaylistID = MoqYtId, TrackID = MoqYtRemoveId });
            await MusicDBApi.DeleteSpotifyTrack(MoqTrackId);
            await MusicDBApi.DeleteYouTubeTrack(MoqYtTrackId);
            await MusicDBApi.DeleteYouTubeTrack(MoqYtRemoveId);
            await MusicDBApi.DeleteSpotifyPlaylist(MoqSpId);
            await MusicDBApi.DeleteYouTubePlaylists(new YoutubePlaylistsModel { PlaylistID = MoqYtId, Name = "Moq Test" });
            try
            {
                using var cnn = new System.Data.SQLite.SQLiteConnection(
                    "Data Source=./MusicDB.db;foreign keys=true;");
                await cnn.OpenAsync();
                using var cmd = cnn.CreateCommand();
                cmd.CommandText = "DELETE FROM PlaylistSync WHERE SpotifyPlaylistID = @sp OR YTPlaylistID = @yt";
                cmd.Parameters.AddWithValue("@sp", MoqSpId);
                cmd.Parameters.AddWithValue("@yt", MoqYtId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }

        [Test]
        public async Task SyncSpotifyTracksToYoutube_Success_WithMoq()
        {
            var spotifyMock = new Mock<ISpotifyApi>();
            var youtubeMock = new Mock<IYoutubeApi>();
            var sync = new SpotifyToYouTubeSync(youtubeMock.Object, spotifyMock.Object);

            try
            {
                // Seed DB
                await MusicDBApi.PostSpotifyPlaylist(new SpotifyPlaylistsModels { PlaylistID = MoqSpId, Name = "Moq Test", SnapshotID = "snap" });
                await MusicDBApi.PostYouTubePlaylists(new YoutubePlaylistsModel { PlaylistID = MoqYtId, Name = "Moq Test" });
                await MusicDBApi.PostPlaylistSync(MoqYtId, MoqSpId, Direction.Spotify, Sync.On);

                await MusicDBApi.PostSpotifyTrack(new SpotifyTracks { TrackID = MoqTrackId, TrackName = MoqTrackName, ArtistName = MoqArtistName });
                await MusicDBApi.PostSpotifyTrackToPlaylist(new SpotifyPlaylistTracks { PlaylistID = MoqSpId, TrackID = MoqTrackId });

                await MusicDBApi.PostYouTubeTrack(new YouTubeTracks { TrackID = MoqYtTrackId, TrackName = MoqTrackName, ArtistName = MoqArtistName });

                await MusicDBApi.PostYouTubeTrack(new YouTubeTracks { TrackID = MoqYtRemoveId, TrackName = "Orphan", ArtistName = "Orphan" });
                await MusicDBApi.PostYTTrackToPlaylist(new YouTubePlaylistTracks { PlaylistID = MoqYtId, TrackID = MoqYtRemoveId, ID = "orphan_item" });

                // Mocks
                spotifyMock.Setup(s => s.StorePlaylistInfoToDBAsync(MoqSpId))
                    .ReturnsAsync(new List<SpotifyTracks>
                    {
                        new() { TrackID = MoqTrackId, TrackName = MoqTrackName, ArtistName = MoqArtistName }
                    });

                youtubeMock.Setup(y => y.StoreTrackToYouTubeDB(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

                youtubeMock.Setup(y => y.AddTrackToPlaylist(MoqYtId, MoqYtTrackId))
                    .ReturnsAsync("moq_item_id");

                youtubeMock.Setup(y => y.DeleteItemFromPlaylistAsync(MoqYtId, MoqYtRemoveId))
                    .Returns(Task.CompletedTask);

                var result = await sync.SyncSpotifyTracksToYoutube(MoqSpId, false);

                Assert.That(result, Is.True);
                youtubeMock.Verify(y => y.AddTrackToPlaylist(MoqYtId, MoqYtTrackId), Times.Once);
                youtubeMock.Verify(y => y.DeleteItemFromPlaylistAsync(MoqYtId, MoqYtRemoveId), Times.Once);
            }
            finally
            {
                await CleanMoqData();
            }
        }

        [Test]
        public async Task SyncSpotifyTracksToYoutube_NoSyncedPlaylist_ReturnsFalse()
        {
            var spotifyMock = new Mock<ISpotifyApi>();
            var youtubeMock = new Mock<IYoutubeApi>();
            var sync = new SpotifyToYouTubeSync(youtubeMock.Object, spotifyMock.Object);

            spotifyMock.Setup(s => s.StorePlaylistInfoToDBAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<SpotifyTracks>());

            var result = await sync.SyncSpotifyTracksToYoutube("moq_nonexistent_sp");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task SyncYoutubeTracksToSpotify_Success_WithMoq()
        {
            var spotifyMock = new Mock<ISpotifyApi>();
            var youtubeMock = new Mock<IYoutubeApi>();
            var sync = new SpotifyToYouTubeSync(youtubeMock.Object, spotifyMock.Object);

            var spId = "moq_s2s_sp_001";
            var ytId = "moq_s2s_yt_001";

            try
            {
                await MusicDBApi.PostSpotifyPlaylist(new SpotifyPlaylistsModels { PlaylistID = spId, Name = "S2S Test", SnapshotID = "snap" });
                await MusicDBApi.PostYouTubePlaylists(new YoutubePlaylistsModel { PlaylistID = ytId, Name = "S2S Test" });
                await MusicDBApi.PostPlaylistSync(ytId, spId, Direction.YouTube, Sync.On);

                await MusicDBApi.PostYouTubeTrack(new YouTubeTracks { TrackID = "moq_s2s_vid_001", TrackName = "S2S Song", ArtistName = "S2S Artist" });
                await MusicDBApi.PostYTTrackToPlaylist(new YouTubePlaylistTracks { PlaylistID = ytId, TrackID = "moq_s2s_vid_001", ID = "s2s_item" });

                youtubeMock.Setup(y => y.StoreYTPlaylistTracksToDB(ytId))
                    .ReturnsAsync(new List<string> { "moq_s2s_vid_001" });

                spotifyMock.Setup(s => s.SearchForTracks("S2S Song", "S2S Artist"))
                    .ReturnsAsync(new SpotifyTracks { TrackID = "moq_s2s_search", TrackName = "S2S Song", ArtistName = "S2S Artist" });

                spotifyMock.Setup(s => s.AddTrackToPlaylist(spId, It.Is<string[]>(a => a.Contains("moq_s2s_search"))))
                    .ReturnsAsync("snap_s2s");

                spotifyMock.Setup(s => s.DeleteTrackFromPlaylist(spId, It.Is<string[]>(a => a.Length == 0)))
                    .ReturnsAsync((string)null);

                var result = await sync.SyncYoutubeTracksToSpotify(ytId);

                Assert.That(result, Is.True);
                spotifyMock.Verify(s => s.SearchForTracks("S2S Song", "S2S Artist"), Times.Once);
                spotifyMock.Verify(s => s.AddTrackToPlaylist(spId, It.IsAny<string[]>()), Times.Once);
            }
            finally
            {
                try
                {
                    using var cnn = new System.Data.SQLite.SQLiteConnection(
                        "Data Source=./MusicDB.db;foreign keys=true;");
                    await cnn.OpenAsync();
                    using var cmd = cnn.CreateCommand();
                    cmd.CommandText = @"DELETE FROM PlaylistSync WHERE SpotifyPlaylistID = @sp OR YTPlaylistID = @yt;
DELETE FROM YoutubePlaylistTracks WHERE PlaylistID = @yt;
DELETE FROM YouTubeTracks WHERE TrackID = 'moq_s2s_vid_001';
DELETE FROM SpotifyPlaylists WHERE PlaylistID = @sp;
DELETE FROM YouTubePlaylists WHERE PlaylistID = @yt;";
                    cmd.Parameters.AddWithValue("@sp", spId);
                    cmd.Parameters.AddWithValue("@yt", ytId);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch { }
            }
        }

        [Test]
        public async Task SyncPlaylistAsyncWithYTID_NoExistingSync_WithMoq()
        {
            var spotifyMock = new Mock<ISpotifyApi>();
            var youtubeMock = new Mock<IYoutubeApi>();
            var sync = new SpotifyToYouTubeSync(youtubeMock.Object, spotifyMock.Object);

            var ytId = "moq_sy_yt_001";
            var newSpId = "moq_sy_new_sp_001";
            var newSpName = "Moq Synced YT Playlist";

            try
            {
                await MusicDBApi.PostYouTubePlaylists(new YoutubePlaylistsModel { PlaylistID = ytId, Name = newSpName });

                youtubeMock.Setup(y => y.StoreYouTubePlaylistToSQL(ytId))
                    .ReturnsAsync(newSpName);

                youtubeMock.Setup(y => y.StoreYTPlaylistTracksToDB(ytId))
                    .ReturnsAsync(new List<string>());

                spotifyMock.Setup(s => s.CreatePlaylist(newSpName))
                    .ReturnsAsync(new SpotifyPlaylistsModels { PlaylistID = newSpId, Name = newSpName, SnapshotID = "new_snap" });

                spotifyMock.Setup(s => s.SearchForTracks(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync((SpotifyTracks)null);

                var result = await sync.SyncPlaylistAsyncWithYTID(ytId);

                Assert.That(result, Is.True);
                spotifyMock.Verify(s => s.CreatePlaylist(newSpName), Times.Once);
            }
            finally
            {
                try
                {
                    using var cnn = new System.Data.SQLite.SQLiteConnection(
                        "Data Source=./MusicDB.db;foreign keys=true;");
                    await cnn.OpenAsync();
                    using var cmd = cnn.CreateCommand();
                    cmd.CommandText = @"DELETE FROM PlaylistSync WHERE SpotifyPlaylistID = @sp OR YTPlaylistID = @yt;
DELETE FROM SpotifyPlaylists WHERE PlaylistID = @sp;
DELETE FROM YouTubePlaylists WHERE PlaylistID = @yt;";
                    cmd.Parameters.AddWithValue("@sp", newSpId);
                    cmd.Parameters.AddWithValue("@yt", ytId);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch { }
            }
        }
    }
}
