using Spotify_to_YTMusic.Components.Sql.DataModel;

namespace Spotify_to_YTMusic.Interfaces
{
    public interface IMusicDBApi
    {
        Task<(SpotifyTracks Track, string Err)> GetSpotifyTrack(string trackID);
        Task<(SpotifyTracks Track, string Err)> GetSpotifyTrack(string trackName, string artistName);
        Task<(bool Success, string Err)> PostSpotifyTrack(SpotifyTracks spotifyTrack);
        Task<(bool Success, string Err)> PostSpotifyTracks(List<SpotifyTracks> spotifyTrack);
        Task<(bool Success, string Err)> DeleteSpotifyTrack(string trackId);
        Task<(List<string> Tracks, string Err)> GetAllSpotifyTrackInPlaylist(string playlistID);
        Task<(bool Success, string Err)> PostSpotifyTrackToPlaylist(SpotifyPlaylistTracks playlistTrack);
        Task<(bool Success, string Err)> PostSpotifyTracksToPlaylist(List<SpotifyPlaylistTracks> playlistTrack);
        Task<(bool Success, string Err)> DeleteSpotifyTrackFromPlaylist(SpotifyPlaylistTracks playlistTrack);
        Task<(List<SpotifyPlaylistsModels> Playlists, string Err)> GetAllSportifyPlaylists();
        Task<(SpotifyPlaylistsModels Playlist, string Err)> GetOneSportifyPlaylists(string playlistID);
        Task<(bool Success, string Err)> PostSpotifyPlaylist(SpotifyPlaylistsModels playlist);
        Task<(bool Success, string Err)> UpdateSpotifyPlaylistSnapshotID(string playlistID, string snapshotID);
        Task<(bool Success, string Err)> DeleteSpotifyPlaylist(string playlistID);
        Task<(YouTubeTracks Track, string Err)> GetYouTubeTrack(string trackID);
        Task<(bool Success, string Err)> PostYouTubeTrack(YouTubeTracks youtubeTrack);
        Task<(bool Success, string Err)> PostYouTubeTrack(List<YouTubeTracks> youtubeTrack);
        Task<(bool Success, string Err)> DeleteYouTubeTrack(string videoId);
        Task<(YoutubePlaylistsModel Playlist, string Err)> GetOneYTPlaylist(string playlistID);
        Task<(bool Success, string Err)> PostYouTubePlaylists(YoutubePlaylistsModel playlist);
        Task<(bool Success, string Err)> DeleteYouTubePlaylists(YoutubePlaylistsModel playlist);
        Task<(List<YouTubePlaylistTracks> Tracks, string Err)> GetAllYTTracksfromPlaylist(string playlistID);
        Task<(YouTubePlaylistTracks Track, string Err)> GetTrackFromTYPlaylist(string playlistID, string videoID);
        Task<(bool success, string Err)> PostYTTrackToPlaylist(YouTubePlaylistTracks playlistTrack);
        Task<(bool success, string Err)> PostYTTrackToPlaylist(List<YouTubePlaylistTracks> playlistTracks);
        Task<(bool success, string Err)> DeleteYTTrackFromPlaylist(YouTubePlaylistTracks playlistTrack);
        Task<(List<PlaylistSync> PlaylistSync, string Err)> GetAllPlaylistSync(Direction direction);
        Task<(List<PlaylistSync> PlaylistSync, string Err)> GetAllSyncedPlaylists();
        Task<(string PlaylistId, string Err)> GetSyncedPlaylistWithSpotify(string SpotifyPlaylistID);
        Task<(string PlaylistId, string Err)> GetSyncedPlaylistWithYouTube(string YTPlaylistID);
        Task<(bool Sucess, string Err)> PostPlaylistSync(string youtubePlaylistID, string spotifyPlaylistId, Direction Direction, Sync Sync);
        Task<(bool Sucess, string Err)> DeletePlaylistSync(string playlistSyncID);
        Task<(List<YouTubeTracks> Tracks, string Err)> GetUnsyncedTrackToAddYouTube(string spotifyPlaylistId);
        Task<(List<YouTubeTracks> Tracks, string Err)> GetUnsyncedTracksToRemoveYouTube(string youtubePlaylistID);
        Task<(List<YouTubeTracks> Tracks, string Err)> GetUnsyncedTrackToAddSpotify(string youtubePlaylistID);
        Task<(List<SpotifyTracks> Tracks, string Err)> GetUnsyncedTrackToRemoveSpotify(string youtubePlaylistId);
    }
}
