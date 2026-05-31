using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spotify_to_YTMusic.Components
{
    public static class YouTubeScraper
    {
        private const string UserAgent =
               "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
               "AppleWebKit/537.36 (KHTML, like Gecko) " +
               "Chrome/124.0.0.0 Safari/537.36";

        private const string HomepageUrl = "https://music.youtube.com/";
        private const string SearchEndpoint = "https://music.youtube.com/youtubei/v1/search?prettyPrint=false";
        private const string FallbackClientVersion = "1.20240813.01.00"; 

        private static readonly Regex ClientVersionRegex =
            new(@"""INNERTUBE_CLIENT_VERSION"":""([^""]+)""", RegexOptions.Compiled);

        private static readonly Regex VideoIdRegex =
            new(@"""videoId"":""([a-zA-Z0-9_-]{11})""", RegexOptions.Compiled);

        private static readonly HttpClient _http = new()
        {
            DefaultRequestHeaders =
        {
            { "User-Agent", UserAgent },
            { "Accept-Language", "en-US,en;q=0.9" },
            { "Origin", "https://music.youtube.com" },
            { "Referer", "https://music.youtube.com/" },
            { "X-YouTube-Client-Name", "67" },
            // Bypasses GDPR consent wall (relevant for UK/EU)
            { "Cookie", "CONSENT=YES+; SOCS=CAI" },
        },
            Timeout = TimeSpan.FromSeconds(15),
        };
        private static string? _cachedClientVersion;

        private static async Task<string> GetClientVersionAsync()
        {
            if (_cachedClientVersion is not null)
                return _cachedClientVersion;

            try
            {
                var html = await _http.GetStringAsync(HomepageUrl);
                var match = ClientVersionRegex.Match(html);

                if (match.Success)
                {
                    _cachedClientVersion = match.Groups[1].Value;
                    Console.WriteLine($"Client version: {_cachedClientVersion}");
                    return _cachedClientVersion;
                }

                Console.WriteLine($"Could not extract client version, using fallback ({FallbackClientVersion})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Homepage fetch failed ({ex.Message}), using fallback ({FallbackClientVersion})");
            }

            _cachedClientVersion = FallbackClientVersion;
            return _cachedClientVersion;
        }

        public static async Task<string?> GetFirstResultAsync(string query)
        {
            Console.WriteLine($"Searching YouTube Music: \"{query}\"");

            var clientVersion = await GetClientVersionAsync();

            var body = new
            {
                context = new
                {
                    client = new
                    {
                        clientName = "WEB_REMIX",
                        clientVersion,
                        hl = "en",
                        gl = "US",
                    }
                },
                query
            };

            StringContent content = new(
                JsonSerializer.Serialize(body),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            string json;
            try
            {
                var response = await _http.PostAsync(SearchEndpoint, content);
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP error: {ex.Message}");
                return null;
            }

            var match = VideoIdRegex.Match(json);

            if (!match.Success)
            {
                Console.WriteLine(" No videoId found in response.");
                return null;
            }

            var videoId = match.Groups[1].Value;

            return videoId;
        }
    }

}
