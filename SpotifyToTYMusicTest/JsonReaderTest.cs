using Newtonsoft.Json;
using Spotify_to_YTMusic.Components;
using Spotify_to_YTMusic.Config;
using System;
using System.Diagnostics;
using System.Text.Json.Nodes;
using static SpotifyToTYMusicTest.JsonReaderTest;


namespace SpotifyToTYMusicTest;

public class JsonReaderTest
{
    public class Playlists
    { 
        public string PlaylistId { get; set; }
        public string SnapshotId { get; set; }
    }

    MyJsonReader jsonReader = new MyJsonReader();

    [OneTimeSetUp]
    public void Setup()
    {
        List<Playlists> playlists = new List<Playlists>();
        playlists.Add(new Playlists 
        { 
            PlaylistId = "playlistId1",
            SnapshotId = "oldSnapshot",
        });
        string json = System.Text.Json.JsonSerializer.Serialize(playlists);
        File.WriteAllText("testPlaylist.json", "{\"Playlists\": "+ json +"}");
        jsonReader.File = "testPlaylist.json";
    }

    [Test]
    public async Task WriteSpotifySnapshotIdToJsonAsync_WhenPlaylistIdIsNull_ReturnNullAsync()
    {
        string snapshotId = "SnapshotId";
        string playlistId = "";
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
        await jsonReader.WriteSpotifySnapshotIdToJsonAsync(snapshotId, playlistId).ConfigureAwait(false);
        await jsonReader.ReadJsonAsync().ConfigureAwait(false);
        foreach (var item in jsonReader.Playlists)
        {
            if (item.PlaylistId == playlistId)
            {
                Assert.That(item.PlaylistId, Is.EqualTo(playlistId));
                return;
            }
        }
        Assert.Fail();
    }

    [Test]
    public async Task WriteSpotifySnapshotIdToJsonAsync_WhenUpdatingSnapshotIdAsync()
    {
        string snapshotId = "NewSnapshot";
        string playlistId = "playlistId1";
        await jsonReader.WriteSpotifySnapshotIdToJsonAsync(snapshotId, playlistId).ConfigureAwait(false);
        await jsonReader.ReadJsonAsync().ConfigureAwait(false);
        foreach (var item in jsonReader.Playlists)
        {
            if (item.PlaylistId == playlistId)
            {
                Assert.That(item.SnapshotId, Is.EqualTo(snapshotId));
                return;
            }
        }
        Assert.Fail();
    }

    [Test]
    public async Task WriteVideoIDToFile_AddTracksToJsonAsync_FirstTime()
    {
        string playlistId = "playlistId1";
        var result = await WriteVideoIDToFile_AddTracksToJsonAsync(playlistId,
            "videoID1").ConfigureAwait(false);
        Assert.That(result.Item1, Is.True);
        Assert.That(result.Item2, Is.EqualTo(1));
    }

    [Test]
    public async Task WriteVideoIDToFile_AddTracksToJsonAsync_SecondTime()
    {
        string playlistId = "playlistId1";
        var result = await WriteVideoIDToFile_AddTracksToJsonAsync(playlistId,
            "videoID2").ConfigureAwait(false);
        Assert.That(result.Item1, Is.True);
        Assert.That(result.Item2, Is.EqualTo(2));
    }
    public async Task<(bool, int?)> WriteVideoIDToFile_AddTracksToJsonAsync(string playlistId, string VideoID)
    {
        await jsonReader.AddTracksToJsonAsync(playlistId, VideoID).ConfigureAwait(false);
        await jsonReader.ReadJsonAsync().ConfigureAwait(false);
        bool playlistFound = false;
        int playlistIndex = 0;
        foreach (var item in jsonReader.Playlists)
        {
            if (item.PlaylistId == playlistId)
            {
                playlistFound = true;
                break;
            }
            playlistIndex++;
        }

        if (!playlistFound)
        {
            return (false, null);
        }
        foreach (var track in jsonReader.Playlists[playlistIndex].Tracks)
        {
            if(track.Id == VideoID)
            {
                return (true, jsonReader.Playlists[playlistIndex].Tracks.Count);
            }
        }

        return (false, null);
    }


}

