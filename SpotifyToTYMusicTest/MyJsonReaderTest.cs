using Newtonsoft.Json;
using Spotify_to_YTMusic.Config;

namespace SpotifyToTYMusicTest
{
    public class MyJsonReaderTest
    {
        private string _testDir;

        [SetUp]
        public void Setup()
        {
            _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);
        }

        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        [Test]
        public async Task ReadJsonAsync_ParsesClientIdAndSecret()
        {
            var json = JsonConvert.SerializeObject(new { ClientID = "test-id", ClientSecret = "test-secret" });
            var filePath = Path.Combine(_testDir, "config.json");
            await File.WriteAllTextAsync(filePath, json);

            var reader = new MyJsonReader { File = filePath };
            await reader.ReadJsonAsync();

            Assert.That(reader.ClientID, Is.EqualTo("test-id"));
            Assert.That(reader.ClientSecret, Is.EqualTo("test-secret"));
        }

        [Test]
        public async Task ReadJsonAsync_ThrowsWhenFileMissing()
        {
            var reader = new MyJsonReader { File = Path.Combine(_testDir, "nonexistent.json") };
            Assert.ThrowsAsync<FileNotFoundException>(() => reader.ReadJsonAsync());
        }

        [Test]
        public async Task ReadJsonAsync_ThrowsOnInvalidJson()
        {
            var filePath = Path.Combine(_testDir, "bad.json");
            await File.WriteAllTextAsync(filePath, "not-json");

            var reader = new MyJsonReader { File = filePath };
            Assert.Throws<JsonReaderException>(() => reader.ReadJsonAsync().GetAwaiter().GetResult());
        }

        [Test]
        public void JsonsStreamWriter_WritesJsonFile()
        {
            var filePath = Path.Combine(_testDir, "output.json");
            var data = new JsonStruck { ClientID = "write-id", ClientSecret = "write-secret" };

            var reader = new MyJsonReader { File = filePath };
            reader.JsonsStreamWriter(data);

            Assert.That(File.Exists(filePath), Is.True);
            var content = File.ReadAllText(filePath);
            var parsed = JsonConvert.DeserializeObject<JsonStruck>(content);
            Assert.That(parsed.ClientID, Is.EqualTo("write-id"));
            Assert.That(parsed.ClientSecret, Is.EqualTo("write-secret"));
        }

        [Test]
        public async Task JsonStreamReader_ReturnsParsedData()
        {
            var json = JsonConvert.SerializeObject(new { ClientID = "stream-id", ClientSecret = "stream-secret" });
            var filePath = Path.Combine(_testDir, "stream.json");
            await File.WriteAllTextAsync(filePath, json);

            var reader = new MyJsonReader { File = filePath };
            var result = await reader.JsonStreamReader();

            Assert.That(result.ClientID, Is.EqualTo("stream-id"));
            Assert.That(result.ClientSecret, Is.EqualTo("stream-secret"));
        }
    }
}
