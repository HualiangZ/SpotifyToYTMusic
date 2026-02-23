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

namespace Spotify_to_YTMusic.Components.Sql
{
    public class MusicDBApi
    {
        static string cnnString = "Data Source=./MusicDB.db;foreign keys=true;";

        public static (SpotifyTracks Track, string Err) GetSpotifyTrack(string trackID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<SpotifyTracks>("select * from SpotifyTracks where TrackID = @TrackID", new { TrackID = trackID }).ToList()[0], null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }
            }
        }

        public static (SpotifyTracks Track, string Err) GetSpotifyTrack(string trackName, string artistName)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<SpotifyTracks>("select * from SpotifyTracks where TrackName = @TrackName AND ArtistName = ArtistName", 
                        new { TrackName = trackName, ArtistName = artistName }).ToList()[0], null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }
            }
        }

        public static (bool Success, string Err) PostSpotifyTrack(SpotifyTracks spotifyTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into SpotifyTracks (TrackID, TrackName, ArtistName) values (@TrackID, @TrackName, @ArtistName)", spotifyTrack);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }

            }
        }
        public static (bool Success, string Err) DeleteSpotifyTrack(string trackId)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM SpotifyTracks WHERE TrackID = @TrackID", new { TrackID = trackId });
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        }

        public static (List<string> Tracks, string Err) GetAllSpotifyTrackInPlaylist(string playlistID)
        {
            using(IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<string>("select TrackID from SpotifyPlaylistTracks where PlaylistID = @PlaylistID", new {PlaylistID = playlistID}).ToList(), null);
                }
                catch (Exception ex) 
                {
                    return (null, ex.Message);
                }
                
            }
        }

        public static (bool Success, string Err) PostSpotifyTrackToPlaylist(SpotifyPlaylistTracks playlistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into SpotifyPlaylistTracks (PlaylistID, TrackID) values (@PlaylistID, @TrackID)", playlistTrack);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
                
            }
        }

        public static (bool Success, string Err) DeleteSpotifyTrackFromPlaylist(SpotifyPlaylistTracks playlistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM SpotifyPlaylistTracks WHERE PlaylistID = @PlaylistID AND TrackID = @TrackID ", playlistTrack);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        }

        public static (List<SpotifyPlaylistsModels> Playlists, string Err) GetAllSportifyPlaylists()
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<SpotifyPlaylistsModels>("select * from SpotifyPlaylists", new DynamicParameters()).ToList(), null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }

            }
        }

        public static (SpotifyPlaylistsModels Playlist, string Err) GetOneSportifyPlaylists(string playlistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<SpotifyPlaylistsModels>("select * from SpotifyPlaylists where PlaylistID = @PlaylistID", new {PlaylistID = playlistID}).ToList()[0], null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }

            }
        }

        public static (bool Success, string Err) PostSpotifyPlaylist(SpotifyPlaylistsModels playlist)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into SpotifyPlaylists (PlaylistID, SnapshotID, Name) values (@PlaylistID, @SnapshotID, @Name)", playlist);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }

            }
        }

        public static (bool Success, string Err) UpdateSpotifyPlaylistSnapshotID(string playlistID, string snapshotID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("update SpotifyPlaylists set SnapshotID = @SnapshotID where PlaylistID = @PlaylistID", new { PlaylistID = playlistID, SnapshotID = snapshotID }) ;
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }

            }
        }
        public static (bool Success, string Err) DeleteSpotifyPlaylist(string playlistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM SpotifyPlaylists WHERE PlaylistID = @PlaylistID", new { PlaylistID = playlistID });
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        }

        public static (YouTubeTracks Track, string Err) GetYouTubeTrack(string trackID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<YouTubeTracks>("select * from YouTubeTracks where TrackID = @TrackID", new { TrackID = trackID }).ToList()[0], null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }
            }
        }
        public static (bool Success, string Err) PostYouTubeTrack(YouTubeTracks youtubeTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into YouTubeTracks (TrackID, TrackName, ArtistName) values (@TrackID, @TrackName, @ArtistName)", youtubeTrack);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }

            }
        }
        public static (bool Success, string Err) DeleteYouTubeTrack(string videoId)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
               
                try
                {
                    cnn.Execute("DELETE FROM YouTubeTracks WHERE TrackID = @TrackID", new {TrackID = videoId});
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        }

        public static (YoutubePlaylistsModel Playlist, string Err) GetOneYTPlaylist(string playlistID) 
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<YoutubePlaylistsModel>("select * from YouTubePlaylists where PlaylistID = @PlaylistID", new { PlaylistID = playlistID }).ToList()[0], null);
                }catch(Exception ex)
                {
                    return (null, ex.Message);
                }
            }
        }

        public static (bool Success, string Err) PostYouTubePlaylists(YoutubePlaylistsModel playlist)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into YouTubePlaylists (PlaylistID, Name) values (@PlaylistID, @Name)", playlist);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }

            }
        }

        public static (bool Success, string Err) DeleteYouTubePlaylists(YoutubePlaylistsModel playlist)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM YouTubePlaylists WHERE PlaylistID = @PlaylistID AND Name = @Name ", playlist);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        }

        public static (List<YouTubePlaylistTracks> Tracks, string Err) GetAllYTTracksfromPlaylist(string playlistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<YouTubePlaylistTracks>("select * from YouTubePlaylistTracks where PlaylistID = @PlaylistID", new {PlaylistID = playlistID}).ToList(), null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }

            }
        }

        public static (YouTubePlaylistTracks Track, string Err) GetTrackFromTYPlaylist(string playlistID, string videoID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<YouTubePlaylistTracks>("select * from YouTubePlaylistTracks where PlaylistID = @PlaylistID and TrackID = @TrackID", new { PlaylistID = playlistID, TrackID = videoID}).ToList()[0], null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }

            }
        }

        public static (bool success, string Err) PostYTTrackToPlaylist(YouTubePlaylistTracks playlistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into YouTubePlaylistTracks (PlaylistID, TrackID, ID) values (@PlaylistID, @TrackID, @ID)", playlistTrack);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }

            }
        }


        public static (bool success, string Err) DeleteYTTrackFromPlaylist(YouTubePlaylistTracks playlistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM YouTubePlaylistTracks WHERE PlaylistID = @PlaylistID AND TrackID = @TrackID ", playlistTrack);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        }
        public static (List<PlaylistSync> PlaylistSync, string Err) GetAllSyncedPlaylists()
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<PlaylistSync>("select * from PlaylistSync").ToList(), null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }
            }
        }

        public static (string PlaylistId, string Err) GetSyncedPlaylistWithSpotify(string SpotifyPlaylistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<string>("select YTPlaylistID from PlaylistSync where SpotifyPlaylistID = @SpotifyPlaylistID", new { SpotifyPlaylistID = SpotifyPlaylistID }).ToList()[0], null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }
            }
        }

        public static (string PlaylistId, string Err) GetSyncedPlaylistWithYouTube(string YTPlaylistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<string>("select SpotifyPlaylistID from PlaylistSync where YTPlaylistID = @YTPlaylistID", new { YTPlaylistID = YTPlaylistID }).ToList()[0], null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }
            }
        }

        public static (bool Sucess, string Err) PostPlaylistSync(string youtubePlaylistID, string spotifyPlaylistId)
        {
            PlaylistSync playlistSync = new PlaylistSync();
            playlistSync.YTPlaylistID = youtubePlaylistID;
            playlistSync.SpotifyPlaylistID = spotifyPlaylistId;
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into PlaylistSync (YTPlaylistID, SpotifyPlaylistID) values (@YTPlaylistID, @SpotifyPlaylistID)", playlistSync);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }

            }
        }

        public static (bool Sucess, string Err) DeletePlaylistSync(string playlistSyncID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM PlaylistSync WHERE SyncID = @SyncID", new { syncID = playlistSyncID });
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        }

        public static (List<YouTubeTracks> Tracks, string Err) GetUnsyncedTrackToAddYouTube(string spotifyPlaylistId)
        {
            string YTPlaylistID = GetSyncedPlaylistWithSpotify(spotifyPlaylistId).PlaylistId;
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<YouTubeTracks>(
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
                        new { SpotifyPlaylistID = spotifyPlaylistId, YoutubePlaylistID = YTPlaylistID})
                        .ToList(), null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }
            }
        }

        public static (List<YouTubeTracks> Tracks, string Err) GetUnsyncedTracksToRemoveYouTube(string youtubePlaylistID)
        {
            string spotifyPlaylistID = GetSyncedPlaylistWithYouTube(youtubePlaylistID).PlaylistId;
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
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
                        new { SpotifyPlaylistID = spotifyPlaylistID, YoutubePlaylistID = youtubePlaylistID })
                        .ToList(), null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Get GetUnsyncedTracksToRemoveYouTube error " + ex.Message);
                    return (null, ex.Message);
                }
            }
        }

        public static (List<YouTubeTracks> Tracks, string Err) GetUnsyncedTrackToAddSpotify(string youtubePlaylistID)
        {
            string spotifyPlaylistID = GetSyncedPlaylistWithYouTube(youtubePlaylistID).PlaylistId;
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
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
                        new { SpotifyPlaylistID = spotifyPlaylistID, YoutubePlaylistID = youtubePlaylistID })
                        .ToList(), null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Get GetUnsyncedTrackToAddSpotify error " + ex.Message);
                    return (null, ex.Message);
                }
            }
        }

        public static (List<SpotifyTracks> Tracks, string Err) GetUnsyncedTrackToRemoveSpotify(string youtubePlaylistId)
        {
            string spotifyPlaylistId = GetSyncedPlaylistWithYouTube(youtubePlaylistId).PlaylistId;
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return (cnn.Query<SpotifyTracks>(
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
                        .ToList(), null);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }
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
