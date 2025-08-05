using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

[assembly: InternalsVisibleTo("JsonReaderTest")]
namespace Spotify_to_YTMusic.Config
{
    internal class MyJsonReader
    {
        public string SpotifyClientID { get; set; }
        public string SpotifyClientSecret { get; set; }
        public List<PlaylistsStruct> Playlists {  get; set; }


        public virtual async Task<JsonStruck> JsonStreamReader()
        {
            StreamReader reader = new StreamReader("config.json");
            string json = await reader.ReadToEndAsync().ConfigureAwait(false);
            JsonStruck data = JsonConvert.DeserializeObject<JsonStruck>(json);
            reader.Close();
            return data;
        }

        public virtual void JsonsStreamWriter(JsonStruck data)
        {
            StreamWriter writer = new StreamWriter("config.json");
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, data);
            writer.Close();
        }
        public async Task ReadJsonAsync() 
        {
            JsonStruck data = await JsonStreamReader().ConfigureAwait(false);
            this.SpotifyClientID = data.SpotifyClientID;
            this.SpotifyClientSecret = data.SpotifyClientSecret;
            this.Playlists = data.Playlists;
        }

        public async Task WriteSpotifySnapshotIdToJsonAsync(string spotifySnapshotId, string playlistId)
        {
            JsonStruck data = await JsonStreamReader().ConfigureAwait(false);

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
                    item.SnapshotId = spotifySnapshotId;
                    playlistIdFound = true;
                    break;
                }
            }

            if (!playlistIdFound)
            {
                data.Playlists.Add(new PlaylistsStruct()
                {
                    PlaylistId = playlistId,
                    SnapshotId = spotifySnapshotId,
                });
            }

            JsonsStreamWriter(data);
        }


        public async Task<string> GetPlaylistSnapshotIdAsync(string playlistId)
        {
            JsonStruck data = await JsonStreamReader().ConfigureAwait(false);

            foreach (var item in data.Playlists)
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
