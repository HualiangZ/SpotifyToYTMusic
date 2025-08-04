using Spotify_to_YTMusic.Components;
using System.Diagnostics.Contracts;
using System.Net;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace SpotifyToTYMusicTest
{
    public class Tests
    {
        public class HttpMessageHandelerMock : HttpMessageHandler
        {
            private readonly HttpStatusCode httpStatusCode;
            private int count = 0;
            private HttpResponseMessage[]? responses;

            public HttpMessageHandelerMock(HttpStatusCode httpStatusCode)
            {
                this.httpStatusCode = httpStatusCode;
            }

            public HttpMessageHandelerMock(params HttpResponseMessage[] responses)
            {
                this.responses = responses;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if(responses != null)
                {
                    return Task.FromResult(responses[count++ % responses.Length]);
                }

                return Task.FromResult(new HttpResponseMessage()
                {
                    StatusCode = httpStatusCode,
                });
            }
        }

        [Test]
        public async Task ReturnNull_GetAccessTokenAsync_When400()
        {
            var client = new HttpClient(new HttpMessageHandelerMock(HttpStatusCode.BadRequest));
            var api = new SpotifyApi(client);
            await api.GetAccessTokenAsync().ConfigureAwait(false);

            Assert.Null(api.AccessToken);

        }

        [Test]
        public async Task ReturnSuccess_GetAccessTokenAsync_When200()
        {
            var client = new HttpClient(new HttpMessageHandelerMock(new HttpResponseMessage() 
            { 
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent ("{\"access_token\": \"token\"}")

            }));
            var api = new SpotifyApi(client);
            await api.GetAccessTokenAsync().ConfigureAwait(false);

            Assert.NotNull(api.AccessToken);
            Assert.That(api.AccessToken, Is.EqualTo("token"));
        }

        [Test]
        public async Task ReturnNull_GetPlaylistSnapshotIdAsync_When400()
        {
            var client = new HttpClient(new HttpMessageHandelerMock(HttpStatusCode.BadRequest));
            var api = new SpotifyApi(client);

            var result = await api.GetPlaylistSnapshotIdAsync("playlistId").ConfigureAwait(false);
            Assert.Null(result);
        }

        [Test]
        public async Task ReturnSuccess_GetPlaylistSnapshotIdAsync_When200()
        {
            var client = new HttpClient(new HttpMessageHandelerMock(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"snapshot_id\": \"snapshotid\"}")

            }));
            var api = new SpotifyApi(client);
            var result = await api.GetPlaylistSnapshotIdAsync("playlistId").ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.That(result, Is.EqualTo("snapshotid"));
        }

        [Test]
        public async Task FirstBadRequest_ThenGetNewAccessToken_()
        {
            var firstResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var secondResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"snapshot_id\": \"snapshotid\"}")
            };
            var handler = new HttpMessageHandelerMock(firstResponse, secondResponse);
            var client = new HttpClient(handler);

            var api = new SpotifyApi(client) 
            {
                AccessToken = "OldToken",
                GetAccessTokenAsync = () =>
                {
                    service.AccessToken = "newToken";
                    return Task.CompletedTask;
                }
            };
            var result = await api.GetPlaylistSnapshotIdAsync("playlistId").ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.That(result, Is.EqualTo("snapshotid"));
        }
    }
}