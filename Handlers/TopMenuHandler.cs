using ADMP.code;
using LibVLCSharp.Shared;
using Microsoft.Win32;
using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ADMP
{
    public class TopMenuHandler
    {
        MainWindow MainWindow { get; set; }
        LibVLCSharp.Shared.MediaPlayer MediaPlayer { get; set; }

        public TopMenuHandler(MainWindow mainWindow, LibVLCSharp.Shared.MediaPlayer mediaPlayer)
        {
            this.MainWindow = mainWindow;
            this.MediaPlayer = mediaPlayer;
        }

        public async void OpenFile(string? filePath = null)//function has an optional parameter, if the filepath isnt sepcified, it opens an open file dialog
        {
            Debug.WriteLine("Attempting to open file...");
            bool wasFilePathSet = true;//so that I know whether the filepath was set from ofd or not

            if (filePath is null)
            {
                OpenFileDialog ofd = new()
                {
                    Multiselect = false,
                    DefaultExt = ".mp4",
                    Filter = "Video files|*.mp4;*.mkv"
                };

                bool? result = ofd.ShowDialog();

                if (result is not null && !(Boolean)result)
                {
                    Debug.WriteLine("File opening canceled/was unsuccessful");
                    return;
                }

                filePath = ofd.FileName;
                wasFilePathSet = false;
            }

            if (!File.Exists(filePath))
            {
                Debug.WriteLine("File not found/not accessible!");
                return;
            }

            Debug.WriteLine($"File ({filePath}) opened successfuly!");

            Media media = new(MainWindow.libVLC, filePath, FromType.FromPath);
            MainWindow.currentMedia = media;

            await media.Parse();

            MediaPlayer.Play(media);

            if (!wasFilePathSet)//adding to recent files, BUT only if it's not already being opened from the recent file list
            {
                bool isAlreadyInRecent = false;
                foreach (string existingFilePath in MainWindow.settingsHandler.AppSettings.LastOpened)
                {
                    if (filePath == existingFilePath)
                    {
                        isAlreadyInRecent = true;
                        break;
                    }
                }
                if (isAlreadyInRecent)
                {
                    Debug.WriteLine("File is already in recent files list, not adding again...");
                    return;
                }
                Debug.WriteLine("Adding file to recent files list...");
                MainWindow.settingsHandler.AppSettings.AddRecent(filePath);
                MainWindow.AddToRecentFilesList(filePath);
                MainWindow.RecentlyOpenedList.Visibility = Visibility.Visible;
            }
            //getting embedded subtitles(if there are any)
            _ = MainWindow.GetEmbeddedSubtitleTracks();

            string[] fileNames = filePath.Split("\\");
            string fileName = fileNames[fileNames.Length - 1];

            string mediaDurationString = ADMPUtils.GetMediaDurationString(media.Duration);

            MainWindow.PlayPauseButtonImage.Source = new BitmapImage(new Uri("icons/pause_btn.png", UriKind.Relative)); ;
            MainWindow.TopOverlayFilenameText.Text = fileName;
            MainWindow.TopOverlayDurationText.Text = mediaDurationString;
            MainWindow.isPlaying = true;

            Timer labelsTimer = new Timer(5000);
            labelsTimer.Elapsed += (object? sender, ElapsedEventArgs e) =>
            {
                MainWindow.TopOverlay.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new labelHideDelegate(() =>
                {
                    MainWindow.TopOverlayFilenameText.Visibility = Visibility.Hidden;
                    MainWindow.TopOverlayDurationText.Visibility = Visibility.Hidden;
                }));
            };
            labelsTimer.Start();
        }

        public async Task LoadSubtitleFile()
        {
            if (MainWindow.currentMedia is null)
            {
                Debug.WriteLine("Cannot load subtitle file, because no media is playing!");
                return;
            }

            OpenFileDialog ofd = new()
            {
                Multiselect = false,
                DefaultExt = ".srt",
                Filter = "Subtitle files|*.srt;*.sub;*."
            };

            bool? result = ofd.ShowDialog();

            if (result == false)
            {
                Debug.WriteLine("Subtitle file selection canceled/was unsuccessful");
                return;
            }

            string subtitlePath = ofd.FileName;
            if (!File.Exists(subtitlePath))
            {
                Debug.WriteLine("Subtitle file not found!");
                return;
            }
            var parser = new SubtitlesParser.Classes.Parsers.SubParser();
            List<SubtitleItem> items = [];

            using (var fileStream = File.OpenRead(subtitlePath))
            {
                try
                {
                    var mostLikelyFormat = parser.GetMostLikelyFormat(subtitlePath);
                    items = parser.ParseStream(fileStream, Encoding.UTF8, mostLikelyFormat);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error parsing subtitle file: " + ex.Message);
                    return;
                }
            }
            MainWindow.currentExternalSubtitles = items;

            int lastTag = 1;
            if (MainWindow.currentSubtitles.Count > 0)
            {
                Debug.WriteLine("Existing subtitle tracks found, determining last tag..."); //making sure the external subtitles are added BEHIND the embedded ones
                lastTag = MainWindow.currentSubtitles!.MaxBy(t => t.Tag).Tag;
            }
            var subtitleTrack = new MainWindow.SubtitleTrack(lastTag, "external", false)
            {
                SubtitleItems = items
            };
            MainWindow.currentSubtitles.Add(subtitleTrack);
            Debug.WriteLine("current subtitle count: " + MainWindow.currentSubtitles.Count);

            _ = MainWindow.GenerateSubtitleTracks();

            MainWindow.mainMediaPlayer.SetSpu(-1);//disabling embedded subtitles

            Timer subUpdateTimer = new(100)
            {
                AutoReset = true
            };
            subUpdateTimer.Elapsed += SubtitleUpdateCall;
            subUpdateTimer.Start();
            MainWindow.activeTimers.Add(subUpdateTimer);
        }
        public void SubtitleUpdateCall(object? sender, ElapsedEventArgs e)
        {
            if (MainWindow.currentMedia != null && MainWindow.isPlaying)
            {
                MainWindow.SubtitleDisplay.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SubUpdateDelegate(SubtitleUpdate));
            }
        }
        public delegate void SubUpdateDelegate();
        private void SubtitleUpdate()
        {

            double position = Convert.ToDouble(MainWindow.mainMediaPlayer.Position);
            long duration = MainWindow.mainMediaPlayer.Media.Duration;

            long actualPosition = Convert.ToInt64(Convert.ToDouble(duration) * position);

            int temp = 0;
            if (MainWindow.currentExternalSubtitles.Count > 0)
            {
                temp = MainWindow.currentExternalSubtitles[0].StartTime;
            }

            foreach (SubtitleItem subtitle in MainWindow.currentExternalSubtitles)
            {
                if (actualPosition >= subtitle.StartTime && actualPosition <= subtitle.EndTime)
                {
                    MainWindow.SubtitleDisplay.Visibility = Visibility.Visible;
                    MainWindow.SubtitleDisplay.Text = string.Join("\n", subtitle.Lines);
                    return;
                }
            }
            MainWindow.SubtitleDisplay.Visibility = Visibility.Hidden;
        }

        delegate void labelHideDelegate();
    }
}
