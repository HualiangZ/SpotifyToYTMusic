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

        public static SpotifyTracks GetSpotifyTrack(string trackID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<SpotifyTracks>("select * from SpotifyTracks where TrackID = @TrackID", new { TrackID = trackID }).ToList()[0];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GET " + ex.Message);
                    return null;
                }
            }
        }
        public static void PostSpotifyTrack(SpotifyTracks spotifyTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into SpotifyTracks (TrackID, TrackName, ArtistName) values (@TrackID, @TrackName, @ArtistName)", spotifyTrack);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Post " + ex.Message);
                }

            }
        }
        public static void DeleteSpotifyTrack(SpotifyTracks playlistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM SpotifyTracks WHERE TrackID = @TrackID AND TrackName = @TrackName AND ArtistName = @ArtistName", playlistTrack);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Delete " + ex.Message);
                }
            }
        }

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

        public static SpotifyPlaylistsModels GetOneSportifyPlaylists(string playlistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<SpotifyPlaylistsModels>("select * from SpotifyPlaylists where PlaylistID = @PlaylistID", new {PlaylistID = playlistID}).ToList()[0];
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
                    cnn.Execute("insert into SpotifyPlaylists (PlaylistID, SnapshotID, Name) values (@PlaylistID, @SnapshotID, @Name)", playlist);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Post " + ex.Message);
                }

            }
        }
        public static void UpdateSpotifyPlaylistSnapshotID(string playlistID, string snapshotID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("update SpotifyPlaylists set SnapshotID = @SnapshotID where PlaylistID = @PlaylistID", new { PlaylistID = playlistID, SnapshotID = snapshotID }) ;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Post " + ex.Message);
                }

            }
        }
        public static void DeleteSpotifyPlaylist(string playlistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM SpotifyPlaylists WHERE PlaylistID = @PlaylistID", new { PlaylistID = playlistID });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Delete " + ex.Message);
                }
            }
        }

        public static YouTubeTracks GetYouTubeTrack(string trackID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<YouTubeTracks>("select * from YouTubeTracks where TrackID = @TrackID", new { TrackID = trackID }).ToList()[0];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GET " + ex.Message);
                    return null;
                }
            }
        }
        public static void PostYouTubeTrack(YouTubeTracks spotifyTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into YouTubeTracks (TrackID, TrackName, ArtistName) values (@TrackID, @TrackName, @ArtistName)", spotifyTrack);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Post " + ex.Message);
                }

            }
        }
        public static void DeleteYouTubeTrack(YouTubeTracks playlistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM YouTubeTracks WHERE TrackID = @TrackID AND TrackName = @TrackName AND ArtistName = @ArtistName", playlistTrack);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Delete " + ex.Message);
                }
            }
        }

        public static YoutubePlaylistsModel GetOneYTPlaylist(string playlistID) 
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<YoutubePlaylistsModel>("select * from YouTubePlaylists where PlaylistID = @PlaylistID", new { PlaylistID = playlistID }).ToList()[0];
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

        public static PlaylistSync GetSyncedPlaylistWithSpotify(string SpotifyPlaylistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<PlaylistSync>("select * from PlaylistSync where SpotifyPlaylistID = @SpotifyPlaylistID", new { SpotifyPlaylistID = SpotifyPlaylistID }).ToList()[0];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GET one playlist sync " + ex.Message);
                    return null;
                }
            }
        }

        public static List<PlaylistSync> GetSyncedPlaylistWithYouTube(string YTPlaylistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    return cnn.Query<PlaylistSync>("select * from PlaylistSync where YTPlaylistID = @YTPlaylistID", new { YTPlaylistID = YTPlaylistID }).ToList();
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