namespace Spotify_to_YTMusic.Interfaces
{
    public interface IYouTubeScraper
    {
        Task<string> GetFirstResultAsync(string query);
    }
}
