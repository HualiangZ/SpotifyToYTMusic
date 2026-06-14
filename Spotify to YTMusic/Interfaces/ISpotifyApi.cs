using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components.Sql.DataModel;

namespace Spotify_to_YTMusic.Interfaces
{
    public interface ISpotifyApi
    {
        string AccessToken { get; set; }
        Task<string> GetAuthCodeAsync();
        Task GetAccessTokenAsync();
        Task RefreshAccessToken();
        Task<string> GetUserID();
        Task<SpotifyPlaylistsModels> CreatePlaylist(string name);
        Task<string> StorePlaylistToDB(string playlistId);
        Task<string> GetPlaylistSnapshotIdAsync(string playlistId);
        Task<bool> CheckSnapshotIdChangeAsync(string playlistId);
        Task<JObject> GetTracksInPlaylist(string url);
        Task<List<SpotifyTracks>> StorePlaylistInfoToDBAsync(string playlistId);
        Task<(SpotifyTracks spotifyTracks, SpotifyPlaylistTracks playlistTracks)> AddTracksToSQLPlaylist(string trackName, string artist, string trackID, List<string> spotifyPlaylistTracks, string playlistId);
        Task<string> DeleteTrackFromPlaylist(string playlistId, string[] trackIDs);
        Task<string> AddTrackToPlaylist(string playlistId, string[] trackIDs);
        Task<string> AddTrackToPlaylistchunk(string playlistId, string[] trackIDs);
        Task<SpotifyTracks> SearchForTracks(string trackName, string artistName);
    }
}
