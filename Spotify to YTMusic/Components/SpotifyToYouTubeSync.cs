using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

        public async Task SyncPlaylistAsyncWithSpotifyID(string spotifyPlaylistId)
        {
            string playlistName = await spotifyApi.StorePlaylistToDB(spotifyPlaylistId).ConfigureAwait(false);

            string youtubePlaylistId = MusicDBApi.GetSyncedPlaylistWithSpotify(spotifyPlaylistId);
            if (youtubePlaylistId == null)
            {
                await spotifyApi.StorePlaylistInfoToDBAsync(spotifyPlaylistId).ConfigureAwait(false);
                string playlistId = youtubePlaylistId;
                string newYoutubePlaylistId = await youtubeApi.CreateNewPlaylist(playlistName).ConfigureAwait(false);
                PlaylistSync newPlaylistSync = new PlaylistSync();
                newPlaylistSync.SpotifyPlaylistID = spotifyPlaylistId;
                newPlaylistSync.YTPlaylistID = newYoutubePlaylistId;
                MusicDBApi.PostPlaylistSync(newPlaylistSync);
                await SyncSpotifyTracksToYoutube(spotifyPlaylistId).ConfigureAwait(false);
            }
            else
            {
                PlaylistSync playlistSync = new PlaylistSync();
                playlistSync.SpotifyPlaylistID = spotifyPlaylistId;
                playlistSync.YTPlaylistID = youtubePlaylistId;
                MusicDBApi.PostPlaylistSync(playlistSync);
                youtubeApi.StorePlaylistToDB(playlistName, youtubePlaylistId);
                await SyncSpotifyTracksToYoutube( spotifyPlaylistId).ConfigureAwait(false);
            }

        }

        //spotify -> youtube
        public async Task SyncSpotifyTracksToYoutube(string spotifyPlaylistId)
        {
            List<YouTubeTracks> tracksToBeAdded = MusicDBApi.GetUnsyncedTracksFromSpotify(spotifyPlaylistId);
            string youtubePlaylistID = MusicDBApi.GetSyncedPlaylistWithSpotify(spotifyPlaylistId);

            if (tracksToBeAdded != null) 
            {
                foreach (YouTubeTracks track in tracksToBeAdded) 
                {
                    await youtubeApi.AddTrackToPlaylist(youtubePlaylistID, track.TrackID).ConfigureAwait(false);
                }
            }
            List<YouTubeTracks> tracksToBeRemoved = MusicDBApi.GetUnsyncedTracksFromSpotify(youtubePlaylistID);
            if (tracksToBeRemoved != null)
            {
                foreach (YouTubeTracks track in tracksToBeRemoved)
                {
                    await youtubeApi.DeleteItemFromPlaylistAsync(youtubePlaylistID, track.TrackID).ConfigureAwait(false);
                }
            }

        }

        public async Task SyncPlaylistAsyncWithYTID( string youtubePlaylistID)
        {
            string spotifyPlaylistId = MusicDBApi.GetSyncedPlaylistWithYouTube(youtubePlaylistID);
            bool checkIsYouTubePlaylistinDB = MusicDBApi.GetOneYTPlaylist(youtubePlaylistID) != null;
            if ((spotifyPlaylistId == null && checkIsYouTubePlaylistinDB == false) || (spotifyPlaylistId == "" && checkIsYouTubePlaylistinDB == false)){
                string playlistName = await youtubeApi.StoreYouTubePlaylistToSQL(youtubePlaylistID).ConfigureAwait(false);
                spotifyPlaylistId = spotifyApi.CreatePlaylist(playlistName).Result.PlaylistID;
                PlaylistSync playlistSync = new PlaylistSync();
                playlistSync.YTPlaylistID = youtubePlaylistID;
                playlistSync.SpotifyPlaylistID = spotifyPlaylistId;
                MusicDBApi.PostPlaylistSync(playlistSync);
                await SyncYoutubeTracksToSpotify(youtubePlaylistID).ConfigureAwait(false);
            }
            if(spotifyPlaylistId != null && checkIsYouTubePlaylistinDB == true)
            {
                await SyncYoutubeTracksToSpotify(youtubePlaylistID).ConfigureAwait(false);
            }
        }

        //youtube -> spotify
        public async Task SyncYoutubeTracksToSpotify(string youtubePlaylistId)
        {
            List<SpotifyTracks> spotifyTracks = MusicDBApi.GetUnsyncedTracksFromYoutube(youtubePlaylistId);
            List<string> spotifyTrackId = new List<string>();
            var spotifyId = MusicDBApi.GetSyncedPlaylistWithYouTube(youtubePlaylistId);

            foreach(var item in spotifyTracks)
            {
                spotifyTrackId.Add(item.TrackID);
            }

            await spotifyApi.AddTrackToPlaylist(spotifyId, spotifyTrackId.ToArray()).ConfigureAwait(false);

            List<SpotifyTracks> spotifyTrackToDelete = MusicDBApi.GetUnsyncedTracksFromYoutube(youtubePlaylistId);
            List<string>  spotifyTrackIdToDelet = new List<string>();
            foreach (var item in spotifyTrackToDelete) 
            {
                spotifyTrackIdToDelet.Add(item.TrackID);
            }

            await spotifyApi.DeleteTrackFromPlaylist(spotifyId, spotifyTrackIdToDelet.ToArray()).ConfigureAwait(false);

        }

        //update youtube playlist when spotify snapshot ID chagnes
        public async Task UpdateYTPlaylist(string playlistID)
        {
            if (spotifyApi.CheckSnapshotIdChangeAsync(playlistID).Result)
            {
                await SyncSpotifyTracksToYoutube(playlistID).ConfigureAwait(false);
            }
        }
    }
}
