using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components;
using Spotify_to_YTMusic.Interfaces;
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
    internal class MyJsonReader : IMyJsonReader
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string File {  get; set; }
        public virtual async Task<JsonStruck> JsonStreamReader()
        {
            using StreamReader reader = new StreamReader(File);
            string json = await reader.ReadToEndAsync().ConfigureAwait(false);
            JsonStruck data = JsonConvert.DeserializeObject<JsonStruck>(json);
            return data;
        }

        public virtual void JsonsStreamWriter(JsonStruck data)
        {
            StreamWriter writer = new StreamWriter(File);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, data);
            writer.Close();
        }
        public async Task ReadJsonAsync() 
        {
            JsonStruck data = await JsonStreamReader().ConfigureAwait(false);
            this.ClientID = data.ClientID;
            this.ClientSecret = data.ClientSecret;
        }
    }

    internal sealed class JsonStruck
    { 
        public string ClientID {  get; set; }
        public string ClientSecret { get; set;} 
    }
}
