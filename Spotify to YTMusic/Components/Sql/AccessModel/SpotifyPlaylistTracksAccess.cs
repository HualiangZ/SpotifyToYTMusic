using Dapper;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Spotify_to_YTMusic.Components.Sql.AccessModel
{
    public class SpotifyPlaylistTracksAccess
    {
        static string cnnString = "Data Source=./MusicDB.db";

        public static List<SpotifyPlaylistTracks> GetAllTrackInPlaylist(string playlistID)
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

        public static void PostTrackToPlaylist(SpotifyPlaylistTracks PlaylistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("insert into SpotifyPlaylistTracks (PlaylistID, TrackID) values (@PlaylistID, @TrackID)", PlaylistTrack);
                }
                catch (Exception ex)
                { 
                    Console.WriteLine("Post " + ex.Message);
                }
                
            }
        }


        public static void DeleteTrackFromPlaylist(SpotifyPlaylistTracks PlaylistTrack)
        {
            using (IDbConnection cnn = new SQLiteConnection(cnnString))
            {
                try
                {
                    cnn.Execute("DELETE FROM SpotifyPlaylistTracks WHERE PlaylistID = @PlaylistID AND TrackID = @TrackID ", PlaylistTrack);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Delete " + ex.Message);
                }
            }
        }
    }
}