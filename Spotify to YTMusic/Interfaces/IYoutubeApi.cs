using Spotify_to_YTMusic.Components.Sql.DataModel;

namespace Spotify_to_YTMusic.Interfaces
{
    public interface IYoutubeApi
    {
        Task GetCredential();
        Task<bool> GetChannelRequest();
        Task<string> CreateNewPlaylist(string playlistName);
        Task<string> AddTrackToPlaylist(string playlistId, string videoId);
        Task DeleteItemFromPlaylistAsync(string playlistId, string videoId);
        Task<string> StoreYouTubePlaylistToSQL(string playlistId);
        Task<List<string>> StoreYTPlaylistTracksToDB(string playlistId);
        Task StorePlaylistToDB(string playlistName, string playlistId);
        Task StoreTrackToYouTubeDB(string trackName, string artist);
    }
}
