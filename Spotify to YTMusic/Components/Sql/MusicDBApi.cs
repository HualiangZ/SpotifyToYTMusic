using Dapper;
using Google.Apis.YouTube.v3.Data;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Xunit.Abstractions;

namespace Spotify_to_YTMusic.Components.Sql
{
    public class MusicDBApi
    {
        static string cnnString = "Data Source=./MusicDB.db";

        public static List<SpotifyPlaylistTracks> GetAllSpotifyTrackInPlaylist(string playlistID)
        {
            using(IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<SpotifyPlaylistTracks>("select * from SpotifyPlaylistTracks where PlaylistID = @PlaylistID", new {PlaylistID = playlistID}).ToList();
                }
                catch (Exception ex) 
                {
                    Console.WriteLine("GET "+ex.Message);
                    return null;
                }
                
            }
        }

        public static void PostSpotifyTrackToPlaylist(SpotifyPlaylistTracks playlistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into SpotifyPlaylistTracks (PlaylistID, TrackID) values (@PlaylistID, @TrackID)", playlistTrack);
                }
                catch (Exception ex)
                { 
                    Console.WriteLine("Post " + ex.Message);
                }
                
            }
        }


        public static void DeleteSpotifyTrackFromPlaylist(SpotifyPlaylistTracks playlistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM SpotifyPlaylistTracks WHERE PlaylistID = @PlaylistID AND TrackID = @TrackID ", playlistTrack);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Delete " + ex.Message);
                }
            }
        }

        public static List<SpotifyPlaylistsModels> GetAllSportifyPlaylists()
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<SpotifyPlaylistsModels>("select * from SpotifyPlaylists", new DynamicParameters()).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GET " + ex.Message);
                    return null;
                }

            }
        }

        public static void PostSpotifyPlaylist(SpotifyPlaylistsModels playlist)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into SpotifyPlaylistTracks (PlaylistID, SnapshotID, Name) values (@PlaylistID, @SnapshotID, @Name)", playlist);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Post " + ex.Message);
                }

            }
        }
        public static void DeleteSpotifyPlaylist(SpotifyPlaylistsModels playlist)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM SpotifyPlaylistsModels WHERE PlaylistID = @PlaylistID", playlist);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Delete " + ex.Message);
                }
            }
        }

        public static List<YoutubePlaylistsModel> GetAllYTPlaylist(string playlistID) 
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<YoutubePlaylistsModel>("select * from YouTubePlaylists where PlaylistID = @PlaylistID", new { PlaylistID = playlistID }).ToList();
                }catch(Exception ex)
                {
                    Console.WriteLine("Get " + ex.ToString());
                    return null;
                }
            }
        }

        public static void PostYouTubePlaylists(YoutubePlaylistsModel playlist)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into YouTubePlaylists (PlaylistID, TrackID) values (@PlaylistID, @TrackID)", playlist);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Post " + ex.Message);
                }

            }
        }

        public static void DeleteYouTubePlaylists(YoutubePlaylistsModel playlist)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM YouTubePlaylists WHERE PlaylistID = @PlaylistID AND Name = @Name ", playlist);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Delete " + ex.Message);
                }
            }
        }

        public static List<YouTubePlaylistTracks> GetAllYTTracksfromPlaylist(string playlistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<YouTubePlaylistTracks>("select * from YouTubePlaylistTracks where PlaylistID = @PlaylistID", new {PlaylistID = playlistID}).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GET " + ex.Message);
                    return null;
                }

            }
        }

        public static void PostYTTrackToPlaylist(YouTubePlaylistTracks playlistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into YouTubePlaylistTracks (PlaylistID, TrackID) values (@PlaylistID, @TrackID)", playlistTrack);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Post " + ex.Message);
                }

            }
        }


        public static void DeleteYTTrackFromPlaylist(YouTubePlaylistTracks playlistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM YouTubePlaylistTracks WHERE PlaylistID = @PlaylistID AND TrackID = @TrackID ", playlistTrack);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Delete " + ex.Message);
                }
            }
        }
        public static List<PlaylistSync> GetSyncedPlaylists()
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<PlaylistSync>("select * from PlaylistSync").ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GET All playlist sync " + ex.Message);
                    return null;
                }
            }
        }

        public static List<PlaylistSync> GetoneSyncedPlaylists(string playlistSyncID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<PlaylistSync>("select * from PlaylistSync where SyncID = @SyncID", new {syncID = playlistSyncID }).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GET one playlist sync " + ex.Message);
                    return null;
                }
            }
        }

        public static void PostPlaylistSync(PlaylistSync playlistsyncID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into PlaylistSync (YTPlaylistID, SpotifyPlaylistID) values (@YTPlaylistID, @SpotifyPlaylistID)", playlistsyncID);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Post " + ex.Message);
                }

            }
        }

        public static void DeletePlaylistSync(string playlistSyncID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM PlaylistSync WHERE SyncID = @SyncID", new { syncID = playlistSyncID });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Delete " + ex.Message);
                }
            }
        }

    }
}