using System.Net;
using Spotify_to_YTMusic.Components;

namespace SpotifyToTYMusicTest
{
    [Category("Integration")]
    public class YouTubeScraperTest
    {
        private static HttpClient _originalHttpClient;
        private HttpMessageHandlerMock _handlerMock;
        private HttpClient _mockHttpClient;

        [SetUp]
        public void Setup()
        {
            if (_originalHttpClient == null)
                _originalHttpClient = YouTubeScraper._http;

            var cacheField = typeof(YouTubeScraper).GetField("_cachedClientVersion",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            cacheField.SetValue(null, null);

            _handlerMock = new HttpMessageHandlerMock();
            _mockHttpClient = new HttpClient(_handlerMock)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            YouTubeScraper._http = _mockHttpClient;
        }

        [TearDown]
        public void Cleanup()
        {
            _mockHttpClient?.Dispose();
            _handlerMock?.Dispose();
            if (_originalHttpClient != null)
                YouTubeScraper._http = _originalHttpClient;
        }

        [Test]
        public async Task GetFirstResultAsync_ReturnsVideoId_WhenResponseContainsVideoId()
        {
            var htmlContent = "<html>\"INNERTUBE_CLIENT_VERSION\":\"1.20240813.01.00\"</html>";
            var searchResponse = "{\"items\":[{\"videoId\":\"abc123def45\"}]}";

            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(htmlContent) },
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(searchResponse) }
            );

            var result = await YouTubeScraper.GetFirstResultAsync("test query");

            Assert.That(result, Is.EqualTo("abc123def45"));
        }

        [Test]
        public async Task GetFirstResultAsync_ReturnsNull_WhenNoVideoId()
        {
            var htmlContent = "<html>\"INNERTUBE_CLIENT_VERSION\":\"1.20240813.01.00\"</html>";
            var searchResponse = "{\"items\":[{\"something\":\"else\"}]}";

            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(htmlContent) },
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(searchResponse) }
            );

            var result = await YouTubeScraper.GetFirstResultAsync("test query");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetFirstResultAsync_ReturnsNull_WhenHomepageFails()
        {
            var searchResponse = "{\"items\":[{\"something\":\"else\"}]}";

            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("") },
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(searchResponse) }
            );

            var result = await YouTubeScraper.GetFirstResultAsync("test query");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetFirstResultAsync_ReturnsNull_WhenSearchFails()
        {
            var htmlContent = "<html>\"INNERTUBE_CLIENT_VERSION\":\"1.20240813.01.00\"</html>";

            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(htmlContent) },
                new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("") }
            );

            var result = await YouTubeScraper.GetFirstResultAsync("test query");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetFirstResultAsync_UsesFallbackClientVersion_WhenHomepageHasNoMatch()
        {
            var htmlContent = "<html>no client_version_here</html>";
            var searchResponse = "{\"items\":[{\"videoId\":\"xyz789abc12\"}]}";

            _handlerMock.SetupResponses(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(htmlContent) },
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(searchResponse) }
            );

            var result = await YouTubeScraper.GetFirstResultAsync("test query");

            Assert.That(result, Is.EqualTo("xyz789abc12"));
        }

    }
}
