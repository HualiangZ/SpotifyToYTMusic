using Dapper;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System.Data;
using System.Data.SQLite;

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
                    return cnn.Query<SpotifyPlaylistTracks>("select * from SpotifyPlaylistTracks", new DynamicParameters()).ToList();
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

        public static List<YouTubePlaylistTracks> GetAllYTTracksfromPlaylist(string playlistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<YouTubePlaylistTracks>($"select * from YouTubePlaylistTracks where PlaylistID = {playlistID}", new DynamicParameters()).ToList();
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



    }
}