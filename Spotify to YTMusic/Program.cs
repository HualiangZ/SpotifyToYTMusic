using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify_to_YTMusic.Components;
using Spotify_to_YTMusic.Components.Sql;
using Spotify_to_YTMusic.Components.Sql.DataModel;
using System.Buffers.Text;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Text;

namespace Spotify_to_YTMusic
{
    internal class Program

    {
        static async Task Main(string[] args)
        {
            /*            HttpClient client = new HttpClient();
                        SpotifyApi spotifyAPI = new SpotifyApi(client);
                        await spotifyAPI.GetAccessTokenAsync().ConfigureAwait(false);
                        SpotifyTracks track = new SpotifyTracks();
                        track =  await spotifyAPI.SearchForTracks("Jump", "Blackpink");
                        Console.WriteLine($"{track.TrackName} by {track.ArtistName}: {track.TrackID}");*/

            /*YoutubeApi api = new YoutubeApi();
            await api.AddTrackToPlaylist("PLbqjJZ3RMAtFZhdAnwXI0FJIsrH6rvm9D", "r6Eei81SuqE");*/
            //await api.DeleteItemFromPlaylistAsync("PLbqjJZ3RMAtFZhdAnwXI0FJIsrH6rvm9D", "r6Eei81SuqE");
            //string playlistName = await spotifyAPI.StorePlaylistToDB("3vzc1IWX4yE5txsMCXxGzS").ConfigureAwait(false);
            //await spotifyAPI.StorePlaylistInfoToDBAsync("3vzc1IWX4yE5txsMCXxGzS").ConfigureAwait(false);
            //Console.WriteLine(MusicDBApi.GetUnsyncedTracksFromYoutube("abc").Count);
            /*            foreach (var item in MusicDBApi.GetUnsyncedTracksFromYoutube("abc"))
                        {
                            Console.WriteLine(item.TrackID);
                        }*/
            /*            YoutubeApi api = new YoutubeApi();
                        await api.GetCredential();
                        await api.GetItemInPlaylistAsync("PLbqjJZ3RMAtFZhdAnwXI0FJIsrH6rvm9D");*/
        }

    }
}
