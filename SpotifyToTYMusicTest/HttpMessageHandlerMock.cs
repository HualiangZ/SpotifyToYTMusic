using System.Net;

namespace SpotifyToTYMusicTest
{
    public class HttpMessageHandlerMock : HttpMessageHandler, IDisposable
    {
        private Queue<HttpResponseMessage> _responses = new();
        private HttpResponseMessage _singleResponse;

        public void SetupResponse(HttpStatusCode statusCode, HttpContent content)
        {
            _singleResponse = new HttpResponseMessage(statusCode) { Content = content };
        }

        public void SetupResponses(params HttpResponseMessage[] responses)
        {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_responses.Count > 0)
                return Task.FromResult(_responses.Dequeue());
            return Task.FromResult(_singleResponse ?? new HttpResponseMessage(HttpStatusCode.OK));
        }

        public new void Dispose()
        {
            _singleResponse?.Dispose();
            while (_responses.Count > 0)
                _responses.Dequeue()?.Dispose();
        }
    }
}
