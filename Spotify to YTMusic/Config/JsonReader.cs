using Newtonsoft.Json;
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

        public async Task ReadJsonAsync() 
        {
            StreamReader reader = new StreamReader("config.json");
            string json = await reader.ReadToEndAsync().ConfigureAwait(false);
            JsonStruck data = JsonConvert.DeserializeObject<JsonStruck>(json);
            this.SpotifyClientID = data.SpotifyClientID;
            this.SpotifyClientSecret = data.SpotifyClientSecret;
            reader.Close();
        }

        public async Task WriteJsonAsync(string SpotifySnapshot)
        {
            StreamReader reader = new StreamReader("config.json");
            string json = await reader.ReadToEndAsync().ConfigureAwait(false);
            JsonStruck data = JsonConvert.DeserializeObject<JsonStruck>(json);
            reader.Close();

            data.SpotifySnapshot = SpotifySnapshot;
            StreamWriter writer = new StreamWriter("config.json");
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, data);
            writer.Close();
            

        }

    }

    internal sealed class JsonStruck
    { 
        public string SpotifyClientID {  get; set; }
        public string SpotifyClientSecret { get; set;} 
        public string SpotifySnapshot {  get; set; }
    }

}
