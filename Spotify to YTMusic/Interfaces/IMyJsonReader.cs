using Spotify_to_YTMusic.Config;

namespace Spotify_to_YTMusic.Interfaces
{
    internal interface IMyJsonReader
    {
        string ClientID { get; set; }
        string ClientSecret { get; set; }
        string File { get; set; }
        Task<JsonStruck> JsonStreamReader();
        void JsonsStreamWriter(JsonStruck data);
        Task ReadJsonAsync();
    }
}
