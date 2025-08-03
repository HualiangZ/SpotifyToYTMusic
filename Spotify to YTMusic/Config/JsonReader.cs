using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotify_to_YTMusic.Config
{
    internal class JsonReader
    {
        public string SpotifyClientID { get; set; }
        public string SpotifyClientSecret { get; set; }
        public List<PlaylistsStruct> Playlists {  get; set; }

        public async Task ReadJsonAsync() 
        {
            StreamReader reader = new StreamReader("config.json");
            string json = await reader.ReadToEndAsync().ConfigureAwait(false);
            JsonStruck data = JsonConvert.DeserializeObject<JsonStruck>(json);
            this.SpotifyClientID = data.SpotifyClientID;
            this.SpotifyClientSecret = data.SpotifyClientSecret;
            this.Playlists = data.Playlists;  
            reader.Close();
        }

        public async Task WriteSpotifySnapshotToJsonAsync(string spotifySnapshot, string playlistId)
        {
            StreamReader reader = new StreamReader("config.json");
            string json = await reader.ReadToEndAsync().ConfigureAwait(false);
            JsonStruck data = JsonConvert.DeserializeObject<JsonStruck>(json);
            reader.Close();

            if(playlistId == "")
            {
                Console.WriteLine("no playlist ID entered");
                return;
            }

            bool playlistIdFound = false;

            foreach(var item in data.Playlists)
            {
                if(item.PlaylistId == playlistId)
                {
                    item.SnapshotId = spotifySnapshot;
                    playlistIdFound = true;
                    break;
                }
            }

            if (!playlistIdFound)
            {
                data.Playlists.Add(new PlaylistsStruct()
                {
                    PlaylistId = playlistId,
                    SnapshotId = spotifySnapshot,
                });
            }

            StreamWriter writer = new StreamWriter("config.json");
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, data);
            writer.Close();
        }


        public async Task<string> GetPlaylistSnapshotIdAsync(string playlistId)
        {
            StreamReader reader = new StreamReader("config.json");
            string json = await reader.ReadToEndAsync().ConfigureAwait(false);
            JsonStruck data = JsonConvert.DeserializeObject<JsonStruck>(json);
            reader.Close();

            foreach(var item in data.Playlists)
            {
                if(item.PlaylistId == playlistId)
                {
                    return item.SnapshotId;
                }
            }

            return null;
        }

    }


    internal sealed class JsonStruck
    { 
        public string SpotifyClientID {  get; set; }
        public string SpotifyClientSecret { get; set;} 
        public List<PlaylistsStruct> Playlists {  get; set; }
    }

    internal sealed class PlaylistsStruct 
    { 
        public string PlaylistId {  get; set; }
        public string SnapshotId { get; set; }
    }

}
