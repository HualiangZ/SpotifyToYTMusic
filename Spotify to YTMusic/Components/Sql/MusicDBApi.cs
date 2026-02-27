using Dapper;
using Google.Apis.Util;
using Google.Apis.YouTube.v3.Data;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Spotify_to_YTMusic.Components.Sql
{
    public class MusicDBApi
    {
        static string cnnString = "Data Source=./MusicDB.db;foreign keys=true;Journal Mode=WAL;Synchronous=Normal;busy_timeout=5000";
        private static SQLiteConnection CreateConnection() => new SQLiteConnection(cnnString);
        public static async Task<(SpotifyTracks Track, string Err)> GetSpotifyTrack(string trackID)
        {
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                SpotifyTracks track = await cnn.QueryFirstAsync<SpotifyTracks>("select * from SpotifyTracks where TrackID = @TrackID", new { TrackID = trackID });
                return (track, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

        }

        public static async Task<(SpotifyTracks Track, string Err)> GetSpotifyTrack(string trackName, string artistName)
        {
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                SpotifyTracks track = await cnn.QueryFirstAsync<SpotifyTracks>("select * from SpotifyTracks where TrackName = @TrackName AND ArtistName = ArtistName",
                    new { TrackName = trackName, ArtistName = artistName });
                return (track, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

        }

        public static async Task<(bool Success, string Err)> PostSpotifyTrack(SpotifyTracks spotifyTrack)
        {
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("insert into SpotifyTracks (TrackID, TrackName, ArtistName) values (@TrackID, @TrackName, @ArtistName)", spotifyTrack);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }


        }
        public static async Task<(bool Success, string Err)> DeleteSpotifyTrack(string trackId)
        {
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("DELETE FROM SpotifyTracks WHERE TrackID = @TrackID", new { TrackID = trackId });
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

        }

        public static async Task<(List<string> Tracks, string Err)> GetAllSpotifyTrackInPlaylist(string playlistID)
        {
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                List<string> tracks = (await cnn.QueryAsync<string>(
                    "select TrackID from SpotifyPlaylistTracks where PlaylistID = @PlaylistID", new { PlaylistID = playlistID }
                    )).ToList();
                return (tracks, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

        }

        public static async Task<(bool Success, string Err)> PostSpotifyTrackToPlaylist(SpotifyPlaylistTracks playlistTrack)
        {
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("insert into SpotifyPlaylistTracks (PlaylistID, TrackID) values (@PlaylistID, @TrackID)", playlistTrack);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

        }

        public static async Task<(bool Success, string Err)> DeleteSpotifyTrackFromPlaylist(SpotifyPlaylistTracks playlistTrack)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("DELETE FROM SpotifyPlaylistTracks WHERE PlaylistID = @PlaylistID AND TrackID = @TrackID ", playlistTrack);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

        }

        public static async Task<(List<SpotifyPlaylistsModels> Playlists, string Err)> GetAllSportifyPlaylists()
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                var playlists = (await cnn.QueryAsync<SpotifyPlaylistsModels>("select * from SpotifyPlaylists", new DynamicParameters())).ToList();
                return (playlists, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }


        }

        public static async Task<(SpotifyPlaylistsModels Playlist, string Err)> GetOneSportifyPlaylists(string playlistID)
        {
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                var playlist = await cnn.QueryFirstAsync<SpotifyPlaylistsModels>("select * from SpotifyPlaylists where PlaylistID = @PlaylistID", new { PlaylistID = playlistID });
                return (playlist, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }


        }

        public static async Task<(bool Success, string Err)> PostSpotifyPlaylist(SpotifyPlaylistsModels playlist)
        {
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("insert into SpotifyPlaylists (PlaylistID, SnapshotID, Name) values (@PlaylistID, @SnapshotID, @Name)", playlist);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }


        }

        public static async Task<(bool Success, string Err)> UpdateSpotifyPlaylistSnapshotID(string playlistID, string snapshotID)
        {
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("update SpotifyPlaylists set SnapshotID = @SnapshotID where PlaylistID = @PlaylistID", new { PlaylistID = playlistID, SnapshotID = snapshotID });
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }


        }
        public static async Task<(bool Success, string Err)> DeleteSpotifyPlaylist(string playlistID)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("DELETE FROM SpotifyPlaylists WHERE PlaylistID = @PlaylistID", new { PlaylistID = playlistID });
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

        }

        public static async Task<(YouTubeTracks Track, string Err)> GetYouTubeTrack(string trackID)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                var track = await cnn.QueryFirstAsync<YouTubeTracks>("select * from YouTubeTracks where TrackID = @TrackID", new { TrackID = trackID });
                return (track, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

        }
        public static async Task<(bool Success, string Err)> PostYouTubeTrack(YouTubeTracks youtubeTrack)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("insert into YouTubeTracks (TrackID, TrackName, ArtistName) values (@TrackID, @TrackName, @ArtistName)", youtubeTrack);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }


        }
        public static async Task<(bool Success, string Err)> DeleteYouTubeTrack(string videoId)
        {


            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("DELETE FROM YouTubeTracks WHERE TrackID = @TrackID", new { TrackID = videoId });
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

        }

        public static async Task<(YoutubePlaylistsModel Playlist, string Err)> GetOneYTPlaylist(string playlistID)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                var playlist = await cnn.QueryFirstAsync<YoutubePlaylistsModel>("select * from YouTubePlaylists where PlaylistID = @PlaylistID", new { PlaylistID = playlistID });
                return (playlist, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

        }

        public static async Task<(bool Success, string Err)> PostYouTubePlaylists(YoutubePlaylistsModel playlist)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("insert into YouTubePlaylists (PlaylistID, Name) values (@PlaylistID, @Name)", playlist);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }


        }

        public static async Task<(bool Success, string Err)> DeleteYouTubePlaylists(YoutubePlaylistsModel playlist)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("DELETE FROM YouTubePlaylists WHERE PlaylistID = @PlaylistID AND Name = @Name ", playlist);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

        }

        public static async Task<(List<YouTubePlaylistTracks> Tracks, string Err)> GetAllYTTracksfromPlaylist(string playlistID)
        {
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                List<YouTubePlaylistTracks> tracks = (await cnn.QueryAsync<YouTubePlaylistTracks>(
                    "select * from YouTubePlaylistTracks where PlaylistID = @PlaylistID",
                    new { PlaylistID = playlistID }
                    )).ToList();
                return (tracks, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }


        }

        public static async Task<(YouTubePlaylistTracks Track, string Err)> GetTrackFromTYPlaylist(string playlistID, string videoID)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                var track = await cnn.QueryFirstAsync<YouTubePlaylistTracks>(
                    "select * from YouTubePlaylistTracks where PlaylistID = @PlaylistID and TrackID = @TrackID",
                    new { PlaylistID = playlistID, TrackID = videoID });
                return (track, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }


        }

        public static async Task<(bool success, string Err)> PostYTTrackToPlaylist(YouTubePlaylistTracks playlistTrack)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("insert into YouTubePlaylistTracks (PlaylistID, TrackID, ID) values (@PlaylistID, @TrackID, @ID)", playlistTrack);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }


        }


        public static async Task<(bool success, string Err)> DeleteYTTrackFromPlaylist(YouTubePlaylistTracks playlistTrack)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("DELETE FROM YouTubePlaylistTracks WHERE PlaylistID = @PlaylistID AND TrackID = @TrackID ", playlistTrack);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

        }
        public static async Task<(List<PlaylistSync> PlaylistSync, string Err)> GetAllSyncedPlaylists()
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                List<PlaylistSync> playlistSync = (await cnn.QueryAsync<PlaylistSync>("select * from PlaylistSync")).ToList();
                return (playlistSync, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

        }

        public static async Task<(string PlaylistId, string Err)> GetSyncedPlaylistWithSpotify(string SpotifyPlaylistID)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                var playlistId = await cnn.QueryFirstAsync<string>("select YTPlaylistID from PlaylistSync where SpotifyPlaylistID = @SpotifyPlaylistID", new { SpotifyPlaylistID = SpotifyPlaylistID });
                return (playlistId, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

        }

        public static async Task<(string PlaylistId, string Err)> GetSyncedPlaylistWithYouTube(string YTPlaylistID)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                var playlistId = await cnn.QueryFirstAsync<string>("select SpotifyPlaylistID from PlaylistSync where YTPlaylistID = @YTPlaylistID", new { YTPlaylistID = YTPlaylistID });
                return (playlistId, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

        }

        public static async Task<(bool Sucess, string Err)> PostPlaylistSync(string youtubePlaylistID, string spotifyPlaylistId)
        {

            PlaylistSync playlistSync = new PlaylistSync();
            playlistSync.YTPlaylistID = youtubePlaylistID;
            playlistSync.SpotifyPlaylistID = spotifyPlaylistId;

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("insert into PlaylistSync (YTPlaylistID, SpotifyPlaylistID) values (@YTPlaylistID, @SpotifyPlaylistID)", playlistSync);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }


        }

        public static async Task<(bool Sucess, string Err)> DeletePlaylistSync(string playlistSyncID)
        {

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                await cnn.ExecuteAsync("DELETE FROM PlaylistSync WHERE SyncID = @SyncID", new { syncID = playlistSyncID });
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

        }

        public static async Task<(List<YouTubeTracks> Tracks, string Err)> GetUnsyncedTrackToAddYouTube(string spotifyPlaylistId)
        {

            var YTPlaylist = await GetSyncedPlaylistWithSpotify(spotifyPlaylistId);
            var id = YTPlaylist.PlaylistId;
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                List<YouTubeTracks> tracks = (await cnn.QueryAsync<YouTubeTracks>(
                        "SELECT yt.* " +
                        "FROM SpotifyPlaylistTracks spt " +
                        "JOIN SpotifyTracks st " +
                        "ON st.TrackID = spt.TrackID " +
                        "JOIN YouTubeTracks yt " +
                        "ON yt.TrackName = st.TrackName " +
                        "AND yt.ArtistName = st.ArtistName " +
                        "LEFT JOIN YoutubePlaylistTracks ypt " +
                        "ON ypt.PlaylistID = @YoutubePlaylistID " +
                        "AND ypt.TrackID = yt.TrackID " +
                        "WHERE spt.PlaylistID = @SpotifyPlaylistID " +
                        "AND ypt.TrackID IS NULL",
                        new { SpotifyPlaylistID = spotifyPlaylistId, YoutubePlaylistID = id })
                        ).ToList();
                return (tracks, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

        }

        public static async Task<(List<YouTubeTracks> Tracks, string Err)> GetUnsyncedTracksToRemoveYouTube(string youtubePlaylistID)
        {

            var spotifyPlaylist = await GetSyncedPlaylistWithYouTube(youtubePlaylistID);
            var id = spotifyPlaylist.PlaylistId;
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                return (cnn.Query<YouTubeTracks>(
                        "SELECT yt.* " +
                        "FROM YoutubePlaylistTracks ypt " +
                        "JOIN YouTubeTracks yt " +
                        "ON ypt.TrackID = yt.TrackID " +
                        "LEFT JOIN SpotifyTracks st " +
                        "ON st.TrackName = yt.TrackName " +
                        "AND st.ArtistName = yt.ArtistName " +
                        "LEFT JOIN SpotifyPlaylistTracks spt " +
                        "ON spt.PlaylistID = @SpotifyPlaylistID " +
                        "AND spt.TrackID = st.TrackID " +
                        "WHERE ypt.PlaylistID = @YoutubePlaylistID " +
                        "AND spt.TrackID IS NULL",
                        new { SpotifyPlaylistID = id, YoutubePlaylistID = youtubePlaylistID })
                        .ToList(), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get GetUnsyncedTracksToRemoveYouTube error " + ex.Message);
                return (null, ex.Message);
            }

        }

        public static async Task<(List<YouTubeTracks> Tracks, string Err)> GetUnsyncedTrackToAddSpotify(string youtubePlaylistID)
        {
            var spotifyPlaylistID = await GetSyncedPlaylistWithYouTube(youtubePlaylistID);
            string id = spotifyPlaylistID.PlaylistId;
            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                List<YouTubeTracks> tracks = (await cnn.QueryAsync<YouTubeTracks>(
                    "SELECT yt.* " +
                    "FROM YoutubePlaylistTracks ypt " +
                    "JOIN YouTubeTracks yt " +
                    "ON ypt.TrackID = yt.TrackID " +
                    "LEFT JOIN SpotifyTracks st " +
                    "ON st.TrackName = yt.TrackName " +
                    "AND st.ArtistName = yt.ArtistName " +
                    "LEFT JOIN SpotifyPlaylistTracks spt " +
                    "ON spt.PlaylistID = @SpotifyPlaylistID " +
                    "AND spt.TrackID = st.TrackID " +
                    "WHERE ypt.PlaylistID = @YoutubePlaylistID " +
                    "AND spt.TrackID IS NULL",
                    new { SpotifyPlaylistID = spotifyPlaylistID, YoutubePlaylistID = youtubePlaylistID })
                    ).ToList();
                return (tracks, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get GetUnsyncedTrackToAddSpotify error " + ex.Message);
                return (null, ex.Message);
            }

        }

        public static async Task<(List<SpotifyTracks> Tracks, string Err)> GetUnsyncedTrackToRemoveSpotify(string youtubePlaylistId)
        {
            var spotifyPlaylistId = await GetSyncedPlaylistWithYouTube(youtubePlaylistId);
            string id = spotifyPlaylistId.PlaylistId;

            try
            {
                using var cnn = CreateConnection();
                await cnn.OpenAsync();
                List<SpotifyTracks> tracks = (await cnn.QueryAsync<SpotifyTracks>(
                        "SELECT st.* " +
                        "FROM SpotifyPlaylistTracks spt " +
                        "JOIN SpotifyTracks st " +
                        "ON spt.TrackID = st.TrackID " +
                        "LEFT JOIN YouTubeTracks yt " +
                        "ON LOWER(TRIM(yt.TrackName)) = LOWER(TRIM(st.TrackName)) " +
                        "AND LOWER(TRIM(yt.ArtistName)) = LOWER(TRIM(st.ArtistName)) " +
                        "LEFT JOIN YoutubePlaylistTracks ypt " +
                        "ON ypt.PlaylistID = @YoutubePlaylistID " +
                        "AND ypt.TrackID = yt.TrackID " +
                        "WHERE spt.PlaylistID = @SpotifyPlaylistID " +
                        "AND ypt.TrackID IS NULL ",
                        new { SpotifyPlaylistID = spotifyPlaylistId, YoutubePlaylistID = youtubePlaylistId })
                        ).ToList();
                return (tracks, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

        }

        public static List<string> test1()
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                return (cnn.Query<string>(
                    "SELECT TrackID FROM YoutubePlaylistTracks WHERE TrackID NOT IN (SELECT TrackID FROM YoutubeTracks)").ToList());
            }
        }

    }
}
