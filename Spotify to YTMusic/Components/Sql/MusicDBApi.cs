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
                    List<SpotifyPlaylistTracks> items = cnn.Query<SpotifyPlaylistTracks>("select * from SpotifyPlaylistTracks", new DynamicParameters()).ToList();
                    List<SpotifyPlaylistTracks> output = new List<SpotifyPlaylistTracks>();
                    foreach (SpotifyPlaylistTracks i in items) 
                    {
                        if(i.PlaylistID == playlistID)
                        {
                            output.Add(i);
                        }
                    }
                    return output;
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

        public static List<SpotifyPlaylistsModels> GetAllSportifyPlaylists(string playlistID)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    List<SpotifyPlaylistsModels> items = cnn.Query<SpotifyPlaylistsModels>("select * from SpotifyPlaylists", new DynamicParameters()).ToList();
                    List<SpotifyPlaylistsModels> output = new List<SpotifyPlaylistsModels>();
                    foreach (SpotifyPlaylistsModels i in items)
                    {
                        if (i.PlaylistID == playlistID)
                        {
                            output.Add(i);
                        }
                    }
                    return output;
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
                    var items = cnn.Query<YouTubePlaylistTracks>($"select * from YouTubePlaylistTracks where PlaylistID = {playlistID}", new DynamicParameters()).ToList();
                    List<YouTubePlaylistTracks> output = new List<YouTubePlaylistTracks>();
                    foreach (YouTubePlaylistTracks i in items)
                    {
                        if (i.PlaylistID == playlistID)
                        {
                            output.Add(i);
                        }
                    }
                    return output;
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