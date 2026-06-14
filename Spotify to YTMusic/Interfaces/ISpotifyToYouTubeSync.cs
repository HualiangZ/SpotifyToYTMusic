namespace Spotify_to_YTMusic.Interfaces
{
    public interface ISpotifyToYouTubeSync
    {
        Task Init();
        Task<bool> SyncPlaylistAsyncWithSpotifyID(string spotifyPlaylistId);
        Task<bool> SyncPlaylistAsyncWithYTID(string youtubePlaylistID);
        Task UpdateSpotifyPlaylist();
        Task UpdateYTPlaylist();
    }
}
