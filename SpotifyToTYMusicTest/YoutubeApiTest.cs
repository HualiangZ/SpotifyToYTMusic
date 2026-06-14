using System.Net;
using Spotify_to_YTMusic.Components;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;

namespace SpotifyToTYMusicTest
{
    [Category("Integration")]
    public class YoutubeApiTest
    {
        private const string GapYTVideoId = "abc123def45";
        private const string GapYTPlaylistId = "gap_yt_playlist_001";
        private const string GapYTPlaylistName = "Gap YT Playlist Test";

        private HttpClient _originalHttpClient;

        [SetUp]
        public void Setup()
        {
            _originalHttpClient = YouTubeScraper._http;
        }

        [TearDown]
        public async Task Cleanup()
        {
            if (_originalHttpClient != null)
                YouTubeScraper._http = _originalHttpClient;

            await MusicDBApi.DeleteYouTubeTrack(GapYTVideoId);
            await MusicDBApi.DeleteYouTubePlaylists(new YoutubePlaylistsModel
            {
                PlaylistID = GapYTPlaylistId,
                Name = GapYTPlaylistName
            });
        }

        [Test]
        public void Instance_ReturnsSingleton()
        {
            var instance1 = YoutubeApi.Instance();
            var instance2 = YoutubeApi.Instance();

            Assert.That(instance1, Is.SameAs(instance2));
        }

        [Test]
        public async Task StorePlaylistToDB_InsertsPlaylist()
        {
            await YoutubeApi.Instance().StorePlaylistToDB(GapYTPlaylistName, GapYTPlaylistId);

            var result = await MusicDBApi.GetOneYTPlaylist(GapYTPlaylistId);
            Assert.That(result.Err, Is.Null);
            Assert.That(result.Playlist.Name, Is.EqualTo(GapYTPlaylistName));
            Assert.That(result.Playlist.PlaylistID, Is.EqualTo(GapYTPlaylistId));
        }

        [Test]
        public async Task StoreTrackToYouTubeDB_StoresTrackInDB()
        {
            var handlerMock = new HttpMessageHandlerMock();
            var mockHttpClient = new HttpClient(handlerMock);
            YouTubeScraper._http = mockHttpClient;

            var htmlContent = "<html>\"INNERTUBE_CLIENT_VERSION\":\"1.20240813.01.00\"</html>";
            var searchResponse = "{\"items\":[{\"videoId\":\"" + GapYTVideoId + "\"}]}";

            handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(htmlContent) },
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(searchResponse) }
            );

            await YoutubeApi.StoreTrackToYouTubeDB("Gap YT Store Song", "Gap YT Store Artist");

            var result = await MusicDBApi.GetYouTubeTrack(GapYTVideoId);
            Assert.That(result.Err, Is.Null);
            Assert.That(result.Track.TrackName, Is.EqualTo("Gap YT Store Song"));
            Assert.That(result.Track.ArtistName, Is.EqualTo("Gap YT Store Artist"));
        }
    }
}
