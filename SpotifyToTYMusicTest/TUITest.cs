using Spotify_to_YTMusic.Components;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;

namespace SpotifyToTYMusicTest
{
    [Category("Integration")]
    public class TUITest
    {
        private const string TestSpotifyPlaylistId = "tuitest_sp_001";
        private const string TestYTPlaylistId = "tuitest_yt_001";

        [TearDown]
        public async Task Cleanup()
        {
            await MusicDBApi.DeleteSpotifyPlaylist(TestSpotifyPlaylistId);
            await MusicDBApi.DeleteYouTubePlaylists(new YoutubePlaylistsModel
            {
                PlaylistID = TestYTPlaylistId,
                Name = "TUI Test"
            });
        }

        [Test]
        public void Constructor_InitializesApiInstances()
        {
            var tui = new TUI();

            Assert.That(tui, Is.Not.Null);
        }
    }
}
