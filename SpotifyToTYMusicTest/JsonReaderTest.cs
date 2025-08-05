using Newtonsoft.Json;
using Spotify_to_YTMusic.Config;
using System.Text.Json.Nodes;


namespace SpotifyToTYMusicTest;

public class JsonReaderTest
{
    public class Playlists
    { 
        public string PlaylistId { get; set; }
        public string SnapshotId { get; set; }
    }

    [SetUp]
    public void Setup()
    {
        List<Playlists> playlists = new List<Playlists>();
        playlists.Add(new Playlists 
        { 
            PlaylistId = "1",
            SnapshotId = "oldSnapshot",
        });
        string json = System.Text.Json.JsonSerializer.Serialize(playlists);
        File.WriteAllText("testPlaylist.json", "{\"Playlists\": "+ json +"}");
    }

    internal class JsonReaderMock : MyJsonReader
    {
        public override async Task<JsonStruck> JsonStreamReader()
        {
            StreamReader reader = new StreamReader("testPlaylist.json");
            string json = await reader.ReadToEndAsync().ConfigureAwait(false);
            JsonStruck data = JsonConvert.DeserializeObject<JsonStruck>(json);
            reader.Close();
            return data;
        }
        public override void JsonsStreamWriter(JsonStruck data)
        {
            StreamWriter writer = new StreamWriter("testPlaylist.json");
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, data);
            writer.Close();
        }
    }

    [Test]
    public async Task WriteSpotifySnapshotIdToJsonAsync_WhenPlaylistIdIsNull_ReturnNullAsync()
    {
        string snapshotId = "SnapshotId";
        string playlistId = "";
        JsonReaderMock jsonReader = new JsonReaderMock();
        await jsonReader.WriteSpotifySnapshotIdToJsonAsync(snapshotId, playlistId).ConfigureAwait(false);
        await jsonReader.ReadJsonAsync().ConfigureAwait(false);
        foreach(var item in jsonReader.Playlists)
        {
            if(item.PlaylistId == ""){
                Assert.Fail();
            }
        }
        Assert.Pass();
    }

    [Test]
    public async Task WriteSpotifySnapshotIdToJsonAsync_WhenAddingPlaylistAsync()
    {
        string snapshotId = "SnapshotId2";
        string playlistId = "playlistId2";
        JsonReaderMock jsonReader = new JsonReaderMock();
        await jsonReader.WriteSpotifySnapshotIdToJsonAsync(snapshotId, playlistId).ConfigureAwait(false);
        await jsonReader.ReadJsonAsync().ConfigureAwait(false);
        foreach (var item in jsonReader.Playlists)
        {
            if (item.PlaylistId == "playlistId2")
            {
                Assert.Pass();
            }
        }
        Assert.Fail();
    }

    [Test]
    public async Task WriteSpotifySnapshotIdToJsonAsync_WhenUpdatingSnapshotIdAsync()
    {
        string snapshotId = "NewSnapshot";
        string playlistId = "1";
        JsonReaderMock jsonReader = new JsonReaderMock();
        await jsonReader.WriteSpotifySnapshotIdToJsonAsync(snapshotId, playlistId).ConfigureAwait(false);
        await jsonReader.ReadJsonAsync().ConfigureAwait(false);
        foreach (var item in jsonReader.Playlists)
        {
            if (item.PlaylistId == "1")
            {
                Assert.That(item.SnapshotId, Is.EqualTo(snapshotId));
                return;
            }
        }
        Assert.Fail();
    }
}

