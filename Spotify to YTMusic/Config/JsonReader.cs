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
        public string SpotiftyClientID { get; set; }
        public string SpotiftyClientSecret { get; set; }

        public async Task ReadJson() 
        {
            StreamReader reader = new StreamReader("config.json");
            string json = await reader.ReadToEndAsync().ConfigureAwait(false);
            JsonStruck data = JsonConvert.DeserializeObject<JsonStruck>(json);
            this.SpotiftyClientID = data.SpotiftyClientID;
            this.SpotiftyClientSecret = data.SpotiftyClientSecret;
        }

    }

    internal sealed class JsonStruck
    { 
        public string SpotiftyClientID {  get; set; }
        public string SpotiftyClientSecret { get; set;} 
    }

}
