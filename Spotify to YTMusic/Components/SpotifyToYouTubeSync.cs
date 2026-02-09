using Google.Apis.YouTube.v3.Data;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;


namespace Spotify_to_YTMusic.Components
{
    public class SpotifyToYouTubeSync
    {
        static YoutubeApi youtubeApi;
        static SpotifyApi spotifyApi;
        HttpClient client = new HttpClient();
        SemaphoreSlim semaphore;
        public SpotifyToYouTubeSync()
        {
            youtubeApi = new YoutubeApi();
            spotifyApi = new SpotifyApi(client);
            semaphore = new SemaphoreSlim(1);  
        }
        
        public async Task Init()
        {
            await spotifyApi.GetAccessTokenAsync().ConfigureAwait(false);
            await youtubeApi.GetCredential().ConfigureAwait(false);
        }

        public async Task<bool> SyncPlaylistAsyncWithSpotifyID(string spotifyPlaylistId)
        {
            
   

                string playlistName = await spotifyApi.StorePlaylistToDB(spotifyPlaylistId);
                string youtubePlaylistId = MusicDBApi.GetSyncedPlaylistWithSpotify(spotifyPlaylistId).PlaylistId;
                if (youtubePlaylistId == null)
                {
                
                    string playlistId = youtubePlaylistId;
                    string newYoutubePlaylistId = await youtubeApi.CreateNewPlaylist(playlistName);
                    PlaylistSync newPlaylistSync = new PlaylistSync();
                    newPlaylistSync.SpotifyPlaylistID = spotifyPlaylistId;
                    newPlaylistSync.YTPlaylistID = newYoutubePlaylistId;
                    MusicDBApi.PostPlaylistSync(newPlaylistSync);
                    return await SyncSpotifyTracksToYoutube(spotifyPlaylistId);
                }
                else
                {
                    return await SyncSpotifyTracksToYoutube( spotifyPlaylistId);
                }

        }

        //spotify -> youtube
        public async Task<bool> SyncSpotifyTracksToYoutube(string spotifyPlaylistId)
        {
            await semaphore.WaitAsync();
            try
            {
                await spotifyApi.StorePlaylistInfoToDBAsync(spotifyPlaylistId);
                var tracks = MusicDBApi.GetUnsyncedTrackToAddYouTube(spotifyPlaylistId);
                var youtubePlaylistID = MusicDBApi.GetSyncedPlaylistWithSpotify(spotifyPlaylistId);
                if (youtubePlaylistID.PlaylistId == null)
                {
                    return false;
                }
                if (tracks.Tracks.Count != 0)
                {
                    var tracksToAdd = CheckVideoIdManually(tracks.Tracks);
                    if (tracksToAdd != null)
                    {
                        Console.WriteLine("Adding songs to YouTube playlist please wait...");
                        foreach (YouTubeTracks track in tracksToAdd)
                        {
                            var itemId = await youtubeApi.AddTrackToPlaylist(youtubePlaylistID.PlaylistId, track.TrackID);
                        }
                    }
                }

                var tracksToBeRemoved = MusicDBApi.GetUnsyncedTracksToRemoveYouTube(youtubePlaylistID.PlaylistId);
                if (tracksToBeRemoved.Tracks != null)
                {
                    foreach (YouTubeTracks track in tracksToBeRemoved.Tracks)
                    {
                        await youtubeApi.DeleteItemFromPlaylistAsync(youtubePlaylistID.PlaylistId, track.TrackID);
                    }
                }
                return true;
            }
            finally
            {
                semaphore.Release();
            }
            
        }
        private List<YouTubeTracks> CheckVideoIdManually(List<YouTubeTracks> tracks)
        {
            int count = 1;
            foreach(YouTubeTracks track in tracks)
            {
                Console.WriteLine($"{count}: https://music.youtube.com/watch?v={track.TrackID}, {track.TrackName}");
                count++;
            }
            Console.WriteLine($"Do you want to change the Video Id, the video ID may not be the same as the track name" +
                $" check on youtube to see if it's the same (Y/N) ");
            
            

            string response = Console.ReadLine();
            if(response.ToUpper() == "Y" || response.ToUpper() == "YES")
            {
                bool change = true;
                while (change)
                {
                    Console.WriteLine($"Enter the number you want to change or type 0 if you no longer want to change video IDs");
                    bool validIndex = false;
                    while (!validIndex)
                    {
                        try
                        {
                            int index = Int32.Parse(Console.ReadLine());
                            if (index == 0)
                            {
                                validIndex = true;
                                break;
                            }
                            if (index <= tracks.Count)
                            {
                                tracks = ChangeVideoId(index, tracks);
                                validIndex = true;
                                break;
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Enter a valid number.");
                        }
                    }
                    Console.WriteLine("Do you want to change another track? (Y/N)");
                    if(Console.ReadLine().ToUpper() == "N")
                    {
                        change = false;
                        break;
                    }
                }
            }

            return tracks;
        }

        private List<YouTubeTracks> ChangeVideoId(int index, List<YouTubeTracks> tracks)
        {
            Console.WriteLine("Enter a new VideoId: ");
            string newVideoId = Console.ReadLine();
            YouTubeTracks newTrack = new YouTubeTracks();
            newTrack.TrackID = newVideoId;
            newTrack.TrackName = tracks[index-1].TrackName;
            newTrack.ArtistName = tracks[index-1].ArtistName;

            MusicDBApi.PostYouTubeTrack(newTrack);
            MusicDBApi.DeleteYouTubeTrack(tracks[index - 1].TrackID);
            
            tracks[index - 1] = newTrack;
            return tracks;
        }

        public async Task<bool> SyncPlaylistAsyncWithYTID(string youtubePlaylistID)
        {
            
            var spotifyPlaylistId = MusicDBApi.GetSyncedPlaylistWithYouTube(youtubePlaylistID);
            if (spotifyPlaylistId.PlaylistId == null)
            {
                string playlistName = await youtubeApi.StoreYouTubePlaylistToSQL(youtubePlaylistID).ConfigureAwait(false);
                Console.WriteLine(playlistName);
                SpotifyPlaylistsModels playlist = await spotifyApi.CreatePlaylist(playlistName);
                string playlistId = playlist.PlaylistID;
                PlaylistSync playlistSync = new PlaylistSync();
                playlistSync.YTPlaylistID = youtubePlaylistID;
                playlistSync.SpotifyPlaylistID = playlistId;
                MusicDBApi.PostPlaylistSync(playlistSync);
                return await SyncYoutubeTracksToSpotify(youtubePlaylistID).ConfigureAwait(false);
            }
            if (spotifyPlaylistId.PlaylistId != null)
            {
                return await SyncYoutubeTracksToSpotify(youtubePlaylistID).ConfigureAwait(false);
            }
            return false;
        }

        //youtube -> spotify
        public async Task<bool> SyncYoutubeTracksToSpotify(string youtubePlaylistId)
        {
            List<string> youtubeTrackIDs = await youtubeApi.StoreYTPlaylistTracksToDB(youtubePlaylistId).ConfigureAwait(false);

            var spotifyId = MusicDBApi.GetSyncedPlaylistWithYouTube(youtubePlaylistId).PlaylistId;

            bool isAddSuccess = await AddSyncYoutubeTrackToSpotify(youtubePlaylistId, spotifyId);
            if (!isAddSuccess) 
            {
                return false;
            }

            bool isDeleteSuccess = await DeleteSyncYoutubeTrackToSpotify(youtubePlaylistId, spotifyId, youtubeTrackIDs);
            if (!isDeleteSuccess)
            {
                return false;
            }
            return true;   

        }

        private async Task<bool> DeleteSyncYoutubeTrackToSpotify(string youtubePlaylistId, string spotifyId, List<string> youtubeTrackIDs)
        {
            var trackIDsInSQL = MusicDBApi.GetAllYTTracksfromPlaylist(youtubePlaylistId).Tracks;
            if (trackIDsInSQL == null)
            {
                return false;
            }
            foreach (var item in trackIDsInSQL)
            {
                if (!youtubeTrackIDs.Contains(item.TrackID))
                {
                    MusicDBApi.DeleteYTTrackFromPlaylist(item);
                    MusicDBApi.DeleteYouTubeTrack(item.TrackID);
                }

            }

            var spotifyTrackToDelete = MusicDBApi.GetUnsyncedTrackToRemoveSpotify(youtubePlaylistId);
            
            Console.WriteLine(spotifyTrackToDelete.Err);
            List<string> spotifyTrackIdToDelete = new List<string>();
            foreach (var item in spotifyTrackToDelete.Tracks)
            {
                spotifyTrackIdToDelete.Add(item.TrackID);
            }
            Console.WriteLine(spotifyTrackIdToDelete.Count);
            await spotifyApi.DeleteTrackFromPlaylist(spotifyId, spotifyTrackIdToDelete.ToArray()).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> AddSyncYoutubeTrackToSpotify(string youtubePlaylistId, string spotifyId)
        {
            List<YouTubeTracks> missisngTrack = MusicDBApi.GetUnsyncedTrackToAddSpotify(youtubePlaylistId).Tracks;
            while (missisngTrack.Count != 0)
            {
                List<SpotifyTracks> spotifyTracks = new List<SpotifyTracks>();
                foreach (var track in missisngTrack.ToList())
                {
                    var toBeAdded = await spotifyApi.SearchForTracks(track.TrackName, track.ArtistName);
                    spotifyTracks.Add(toBeAdded);
                    missisngTrack.Remove(track);
                    if (spotifyTracks.Count == 100)
                    {
                        break;
                    }
                }
                List<string> spotifyTrackId = new List<string>();
                if (spotifyId == null)
                {
                    return false;
                }

                foreach (var item in spotifyTracks)
                {
                    spotifyTrackId.Add(item.TrackID);
                }

                await spotifyApi.AddTrackToPlaylist(spotifyId, spotifyTrackId.ToArray()).ConfigureAwait(false);
            }
            return true;
        }

        //update youtube playlist when spotify snapshot ID chagnes
        public async Task UpdateYTPlaylist()
        {

            var spotifyPlaylist = MusicDBApi.GetAllSportifyPlaylists();
            if(spotifyPlaylist.Playlists != null || spotifyPlaylist.Playlists.Count() > 0)
            {
                foreach (var playlist in spotifyPlaylist.Playlists)
                {
                    if (spotifyApi.CheckSnapshotIdChangeAsync(playlist.PlaylistID).Result)
                    {
                        await SyncPlaylistAsyncWithSpotifyID(playlist.PlaylistID);
                    }
                }
            }

        }
    }
}
