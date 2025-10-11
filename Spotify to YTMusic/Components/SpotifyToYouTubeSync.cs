using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System;
using System.Collections.Generic;
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

        public async Task SyncPlaylistAsync(string? youtubePlaylistId, string spotifyPlaylistId)
        {
            string playlistName = await spotifyApi.StorePlaylistToDB(spotifyPlaylistId).ConfigureAwait(false);
            await spotifyApi.StorePlaylistInfoToDBAsync(spotifyPlaylistId).ConfigureAwait(false);
            if (youtubePlaylistId == null)
            {
                string playlistId = youtubePlaylistId;
                string newYoutubePlaylistId = await youtubeApi.CreateNewPlaylist(playlistName).ConfigureAwait(false);
                PlaylistSync newPlaylistSync = new PlaylistSync();
                newPlaylistSync.SpotifyPlaylistID = spotifyPlaylistId;
                newPlaylistSync.YTPlaylistID = newYoutubePlaylistId;
                MusicDBApi.PostPlaylistSync(newPlaylistSync);
                await SyncSpotifyTracksToYoutube(spotifyPlaylistId);
            }
            else
            {
                PlaylistSync playlistSync = new PlaylistSync();
                playlistSync.SpotifyPlaylistID = spotifyPlaylistId;
                playlistSync.YTPlaylistID = youtubePlaylistId;
                MusicDBApi.PostPlaylistSync(playlistSync);
                await SyncSpotifyTracksToYoutube(spotifyPlaylistId);
            }

        }

        //spotify -> youtube
        public async Task SyncSpotifyTracksToYoutube(string spotifyPlaylistId)
        {
            List<YouTubeTracks> tracksToBeAdded = MusicDBApi.GetUnsyncedTracksFromSpotify(spotifyPlaylistId);
            string youtubePlaylistID = MusicDBApi.GetSyncedPlaylistWithSpotify(spotifyPlaylistId);
            Console.WriteLine(youtubePlaylistID);   
            List<YouTubeTracks> tracksToBeRemoved = MusicDBApi.GetUnsyncedTracksFromYoutube(youtubePlaylistID);
            if (tracksToBeAdded != null) 
            {
                Console.WriteLine("running");
                foreach (YouTubeTracks track in tracksToBeAdded) 
                {
                    await youtubeApi.AddTrackToPlaylist(youtubePlaylistID, track.TrackID).ConfigureAwait(false);
                }
            }

            if(tracksToBeRemoved != null)
            {
                foreach (YouTubeTracks track in tracksToBeRemoved)
                {
                    await youtubeApi.DeleteItemFromPlaylistAsync(youtubePlaylistID, track.TrackID).ConfigureAwait(false);
                }
            }

        }

        //youtube -> spotify
        public async Task SyncYoutubeTracksToSpotify(string youtubePlaylistId)
        {
            List<YouTubeTracks> youtubeTracks = MusicDBApi.GetUnsyncedTracksFromYoutube(youtubePlaylistId);
            List<string> spotifyTrackId = new List<string>();
            var spotifyId = MusicDBApi.GetSyncedPlaylistWithYouTube(youtubePlaylistId);

            foreach(var item in youtubeTracks)
            {
                spotifyTrackId.Add(MusicDBApi.GetSpotifyTrack(item.TrackName, item.ArtistName).TrackID);
            }

            await spotifyApi.AddTrackToPlaylist(spotifyId, spotifyTrackId.ToArray());

            List<YouTubeTracks> youtubeTrackToDelete = MusicDBApi.GetUnsyncedTracksFromSpotify(spotifyId);
            foreach (var item in youtubeTrackToDelete) 
            {
                await youtubeApi.DeleteItemFromPlaylistAsync(youtubePlaylistId, item.TrackID).ConfigureAwait(false);
            }

        }
    }
}
