using System;
using System.Text;
using System.Windows;
using System.Timers;

namespace BetterNowPlayingWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Timer aTimer;

        public MainWindow()
        {
            InitializeComponent();
            //Now_Playing.Text = "This is now playing stuff!";
            //Request_List.Text += "This is Request List stuff!\n";
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e) // event method to call on timer
        {
            // Read information from streamlabs chatbot text files and init playingOutput for new text file.
            string requestedBy = System.IO.File.ReadAllText(@"C:\Users\Jesse\AppData\Roaming\Streamlabs\Streamlabs Chatbot\Twitch\Files\RequestedBy.txt").Trim();
            string currentSong = System.IO.File.ReadAllText(@"C:\Users\Jesse\AppData\Roaming\Streamlabs\Streamlabs Chatbot\Twitch\Files\CurrentSong.txt").Trim();
            string playingOutput = "";

            // Split artist and song from rest of string
            string[] substrings = currentSong.Split('-');

            // Trim any whitespace from song and artist after splitting them out
            string song = substrings[1].Trim();
            string artist = substrings[0].Trim();

            if (GlobalThings.previousSong == "" && GlobalThings.previousArtist == "")   // This should only ever resolve to tru on app launch
            {
                // Set previous song and artist so that it does not falsly detect a new song playing on startup
                GlobalThings.previousSong = song;
                GlobalThings.previousArtist = artist;

                // Fill in now playing information so that this is not blank in the console on startup
                GlobalThings.consoleNowPlaying = $"{song} by {artist}";
                Dispatcher.Invoke(() => { Now_Playing.Text = GlobalThings.consoleNowPlaying; });
            };

            if (song == GlobalThings.previousSong || artist == GlobalThings.previousArtist)   // Just update ticker if the song has not changed
            {
                
            }
            else    // when song has changed, we need to update the files and console to reflect this
            {
                if (requestedBy.ToLower() == "fleshbox")    // Don't want the "requested by" tacked on if it's not an actual song request
                {
                    // Update necessary vars for song change
                    GlobalThings.consoleNowPlaying = $"{song} by {artist}";
                    playingOutput = $"                                                             🎶 Currently Playing: {song} by {artist} - Request A Song Using !sr in chat 🎶";   // Whitespace is to pad text scroll in obs
                }
                else
                {
                    // Update necessary vars for song change and new song request
                    GlobalThings.consoleNowPlaying = $"{song} by {artist} | Requested by {requestedBy}";
                    string newSongRequestText = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}] SONG REQUEST: {artist} - {song} | Requested by {requestedBy}" + Environment.NewLine;
                    playingOutput = $"                                                             🎶 Currently Playing: {song} by {artist}, requested by {requestedBy} - Request A Song Using !sr in chat 🎶";   // Whitespace is to pad the text scroll in obs

                    // Log new song request to console and increment line counter for next song request
                    Dispatcher.Invoke(() => { Request_List.Text += newSongRequestText; });

                    // Log all new song requests for later since streamlabs chatbot is deficient
                    System.IO.File.AppendAllText("RequestedSongs.txt", newSongRequestText, Encoding.UTF8);
                }

                // Write info to output file.
                System.IO.File.WriteAllText("NowPlaying.txt", playingOutput, Encoding.UTF8);

                // Update console with currently playing track in the correct position
                Dispatcher.Invoke(() => { Now_Playing.Text = GlobalThings.consoleNowPlaying; });

                // Set previous song and artist to currently playing so that next event will trigger the "same song" resolve
                GlobalThings.previousSong = song;
                GlobalThings.previousArtist = artist;
            }


        }

        private void bob()
        {
            Now_Playing.Text = "bob";
        }

        public static class GlobalThings    // Collection of variables that wen need inside the timed event class that we don't want reinitialized on every event.
        {
            public static string previousSong = ""; // previous song and artist for comparison to detect song changes on each event
            public static string previousArtist = "";
            public static string consoleNowPlaying = "";    // I'll be honest, I wasn't sure where else to init this one...
        }

        private void Start_Button(object sender, RoutedEventArgs e)
        {
            aTimer = new Timer(1000);

            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Enabled = true;
        }

        private void Stop_Button(object sender, RoutedEventArgs e)
        {
            aTimer.Stop();
            aTimer.Dispose();
        }
    }
}
