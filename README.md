# SpotifyToYTMusic

Download and open the program in Visual Basic

Follow the Spotify guide to get your clientID and client secret ([Link](https://developer.spotify.com/documentation/web-api/tutorials/getting-started)) make sure Redirect link has "http://127.0.0.1:8888/callback". Copy the information to config.json</br>
Follow the YouTube API guide to get the YouTube clientID and client secret ([Link](https://developers.google.com/youtube/reporting/guides/registering_an_application)) Using OAuth 2.0. Copy information to YT_client_secret.json</br>
Redirect URI for YouTube</br>
<Img src="https://github.com/HualiangZ/SpotifyToYTMusic/blob/a73f82c3c624acfc3f6ce81cbf327ff63ed9d942/Image/Screenshot%202026-02-28%20133146.png"></br>

Check if both files are on Copy if newer, on Copy to Output Directory.</br>
OR Build the solution, then copy both files to SpotifyToYTMusic > Spotify to YTMusic > bin> Debug >net8.0</br>

Due to YouTube API 10,000 Quota limit per day, the most this can add is ~200 songs per day, progress will be saved so you can try adding remaining songs the next day by repeating the steps below</br>
This project also has Auto sync every 5 minute.
Steps below works both ways, type "2" for YouTube -> Spotify</br>
# Spotify -> YouTube</br>
<img src="https://github.com/HualiangZ/SpotifyToYTMusic/blob/ae8aff94e02509081e10bc388210247ba4a6e752/Image/Screenshot%202026-02-28%20114923.png"></br>
<img src="https://github.com/HualiangZ/SpotifyToYTMusic/blob/74116a4f78593384b124c0a1354300b7f17f4f2c/Image/SpotifyPlaylistID.png"></br>
Type "1" then Enter and paste in your Spotify Playlist ID</br>
<img src="https://github.com/HualiangZ/SpotifyToYTMusic/blob/9072f7737bebf921a0cde6df2587a883112079b1/Image/Screenshot%202026-02-28%20114938.png"></br>
It will then store all tracks from your playlist to a file and get the YouTube Music ID for each track.</br>
It will prompt you to check whether the songs are correct and to confirm whether you want to change the IDs—type "n" to add songs.</br>
<img src="https://github.com/HualiangZ/SpotifyToYTMusic/blob/9072f7737bebf921a0cde6df2587a883112079b1/Image/Screenshot%202026-02-28%20114959.png"></br>
If you typed "y" it will ask you which one you want to change, and then enter the trackID of the song you want to change it to. You can get the track ID from YouTube Url (https://www.youtube.com/watch?v=2etMn0rJpIA) trackID = 2etMn0rJpIA</br>
It will then create a Playlist on YouTube Music and add the tracks to it.</br>
<img src="https://github.com/HualiangZ/SpotifyToYTMusic/blob/46a1f5d9d8655fbb7308beb1f53914b020d242ad/Image/Screenshot%202026-02-28%20120427.png" width="1100"></br>

