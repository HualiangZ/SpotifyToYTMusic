using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotify_to_YTMusic.Components.Sql.DataModel
{
    public enum Direction
    {
        Spotify,
        YouTube
    }
    public enum Sync 
    {
        On = 1,
        Off = 0
    }

    public class PlaylistSync
    {
        public string SpotifyPlaylistID { get; set; }
        public string YTPlaylistID { get; set; }
        public Direction Direction { get; set; }
        public Sync Sync { get; set; }
    }


}
