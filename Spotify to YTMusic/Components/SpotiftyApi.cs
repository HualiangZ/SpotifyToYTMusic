using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Spotify_to_YTMusic.Components
{
    internal class SpotiftyApi
    {
        public string? AccessToken { get; set; }
        private string url = "https://api.spotify.com/v1/";
        public void GetAccessToken()
        {
            string url = "https://accounts.spotify.com/api/token";
            var clientId = "";
            var clientSecret = "";

            var encodeClientId_ClientSecret = Convert.ToBase64String((Encoding.UTF8.GetBytes(string.Format("{0}:{1}", clientId, clientSecret))));
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Accept = "application/json";
            webRequest.Headers.Add("Authorization: Basic " + encodeClientId_ClientSecret);

            var request = ("grant_type=client_credentials");
            byte[] req_bytes = Encoding.ASCII.GetBytes(request);
            webRequest.ContentLength = req_bytes.Length;

            Stream stream = webRequest.GetRequestStream();
            stream.Write(req_bytes, 0, req_bytes.Length);
            stream.Close();

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            string jsonString = "";
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
            jsonString = reader.ReadToEnd();
            reader.Close();

            JObject json = JObject.Parse(jsonString);
            AccessToken = (string)json.GetValue("access_token");
            
        }

        public void GetPlaylist()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + "playlists/5a7q5av1kX3ewlMwGuaQE3/tracks?offset=100&limit=100");
            webRequest.Method = "GET";
            webRequest.Headers.Add("Authorization: Bearer " + AccessToken);

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            string jsonString = "";
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
            jsonString = reader.ReadToEnd();
            reader.Close();
            JObject json = JObject.Parse(jsonString);
            Console.Write(json);
        }
    }
}
