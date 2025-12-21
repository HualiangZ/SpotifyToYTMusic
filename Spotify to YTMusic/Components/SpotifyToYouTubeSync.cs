using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotify_to_YTMusic.Components
{
    public class SpotifyToYouTubeSync
    {
        static YoutubeApi youtubeApi;
        static SpotifyApi spotifyApi;
        HttpClient client = new HttpClient();
        public SpotifyToYouTubeSync()
        {
            youtubeApi = new YoutubeApi();
            spotifyApi = new SpotifyApi(client);
        }
        
        public async Task Init()
        {
            await spotifyApi.GetAccessTokenAsync().ConfigureAwait(false);
        }

        public async Task<bool> SyncPlaylistAsyncWithSpotifyID(string spotifyPlaylistId)
        {
            string playlistName = await spotifyApi.StorePlaylistToDB(spotifyPlaylistId).ConfigureAwait(false);     
            string youtubePlaylistId = MusicDBApi.GetSyncedPlaylistWithSpotify(spotifyPlaylistId).PlaylistId;

            if (youtubePlaylistId == null)
            {
                
                string playlistId = youtubePlaylistId;
                string newYoutubePlaylistId = await youtubeApi.CreateNewPlaylist(playlistName).ConfigureAwait(false);
                PlaylistSync newPlaylistSync = new PlaylistSync();
                newPlaylistSync.SpotifyPlaylistID = spotifyPlaylistId;
                newPlaylistSync.YTPlaylistID = newYoutubePlaylistId;
                MusicDBApi.PostPlaylistSync(newPlaylistSync);
                return await SyncSpotifyTracksToYoutube(spotifyPlaylistId).ConfigureAwait(false);
            }
            else
            {
                return await SyncSpotifyTracksToYoutube( spotifyPlaylistId).ConfigureAwait(false);
            }

        }

        //spotify -> youtube
        public async Task<bool> SyncSpotifyTracksToYoutube(string spotifyPlaylistId)
        {
            await spotifyApi.StorePlaylistInfoToDBAsync(spotifyPlaylistId).ConfigureAwait(false);
            var tracksToBeAdded = MusicDBApi.GetUnsyncedTrackToAddYouTube(spotifyPlaylistId);
            var youtubePlaylistID = MusicDBApi.GetSyncedPlaylistWithSpotify(spotifyPlaylistId);
            if (youtubePlaylistID.PlaylistId == null)
            {
                return false;
            }

            if (tracksToBeAdded.Tracks != null) 
            {
                Console.WriteLine("Adding songs to YouTube playlist please wait...");
                foreach (YouTubeTracks track in tracksToBeAdded.Tracks) 
                {
                    var itemId =  await youtubeApi.AddTrackToPlaylist(youtubePlaylistID.PlaylistId, track.TrackID).ConfigureAwait(false);
                }
            }
            var tracksToBeRemoved = MusicDBApi.GetUnsyncedTracksToRemoveYouTube(youtubePlaylistID.PlaylistId);
            if (tracksToBeRemoved.Tracks != null)
            {
                foreach (YouTubeTracks track in tracksToBeRemoved.Tracks)
                {
                    await youtubeApi.DeleteItemFromPlaylistAsync(youtubePlaylistID.PlaylistId, track.TrackID).ConfigureAwait(false);
                }
            }
            return true;

        }

        public async Task<bool> SyncPlaylistAsyncWithYTID(string youtubePlaylistID)
        {
            var spotifyPlaylistId = MusicDBApi.GetSyncedPlaylistWithYouTube(youtubePlaylistID);
            var checkIsYouTubePlaylistInDB = MusicDBApi.GetOneYTPlaylist(youtubePlaylistID);
            if ((spotifyPlaylistId.PlaylistId == null && checkIsYouTubePlaylistInDB.Playlist == null) 
                || (spotifyPlaylistId.PlaylistId == "" && checkIsYouTubePlaylistInDB.Playlist == null))
            {
                string playlistName = await youtubeApi.StoreYouTubePlaylistToSQL(youtubePlaylistID).ConfigureAwait(false);
                string playlistId = spotifyApi.CreatePlaylist(playlistName).Result.PlaylistID;
                PlaylistSync playlistSync = new PlaylistSync();
                playlistSync.YTPlaylistID = youtubePlaylistID;
                playlistSync.SpotifyPlaylistID = playlistId;
                MusicDBApi.PostPlaylistSync(playlistSync);
                return await SyncYoutubeTracksToSpotify(youtubePlaylistID).ConfigureAwait(false);
            }
            if (spotifyPlaylistId.PlaylistId != null && checkIsYouTubePlaylistInDB.Playlist != null)
            {
                return await SyncYoutubeTracksToSpotify(youtubePlaylistID).ConfigureAwait(false);
            }
            return false;
        }

        //youtube -> spotify
        public async Task<bool> SyncYoutubeTracksToSpotify(string youtubePlaylistId)
        {
            List<SpotifyTracks> spotifyTracks = MusicDBApi.GetUnsyncedTrackToAddSpotify(youtubePlaylistId).Tracks;
            List<string> spotifyTrackId = new List<string>();
            var spotifyId = MusicDBApi.GetSyncedPlaylistWithYouTube(youtubePlaylistId).PlaylistId;

            if(spotifyId == null)
            {
                return false;
            }

            foreach (var item in spotifyTracks)
            {
                spotifyTrackId.Add(item.TrackID);
            }

            await spotifyApi.AddTrackToPlaylist(spotifyId, spotifyTrackId.ToArray()).ConfigureAwait(false);

            List<SpotifyTracks> spotifyTrackToDelete = MusicDBApi.GetUnsyncedTrackToRemoveSpotify(youtubePlaylistId).Tracks;
            List<string> spotifyTrackIdToDelet = new List<string>();
            foreach (var item in spotifyTrackToDelete)
            {
                spotifyTrackIdToDelet.Add(item.TrackID);
            }

            await spotifyApi.DeleteTrackFromPlaylist(spotifyId, spotifyTrackIdToDelet.ToArray()).ConfigureAwait(false);
            return true;
        }

        //update youtube playlist when spotify snapshot ID chagnes
        public async Task UpdateYTPlaylist()
        {
            var semaphore = new SemaphoreSlim(1);
            await semaphore.WaitAsync();
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
            semaphore.Release();
        }
    }
}
