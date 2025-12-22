using ADMP.code;
using LibVLCSharp.Shared;
using Microsoft.Win32;
using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static ADMP.MainWindow;

namespace ADMP
{
    public class TopMenuHandler
    {
        MainWindow mainWindow { get; set; }
        LibVLCSharp.Shared.MediaPlayer mediaPlayer { get; set; }

        public TopMenuHandler(MainWindow mainWindow, LibVLCSharp.Shared.MediaPlayer mediaPlayer)
        {
            this.mainWindow = mainWindow;
            this.mediaPlayer = mediaPlayer;
        }
        public async void OpenFile()
        {
            string? filePath = "";
            Debug.WriteLine("Attempting to open file...");

            OpenFileDialog ofd = new()
            {
                Multiselect = false,
                DefaultExt = ".mp4",
                Filter = "Video files|*.mp4;*.mkv"
            };

            bool? result = ofd.ShowDialog();

            if (!result == true)
            {
                Debug.WriteLine("File opening canceled/was unsuccessful");
            }

            filePath = ofd.FileName;

            if (!File.Exists(filePath))
            {
                Debug.WriteLine("File not found/not accessible!");
                return;
            }

            Debug.WriteLine("File (" + filePath + ") opened successfuly!");

            Media media = new Media(mainWindow.libVLC, filePath, FromType.FromPath);
            mainWindow.currentMedia = media;

            await media.Parse();

            mediaPlayer.Play(media);

            _ = mainWindow.GetEmbeddedSubtitleTracks();

            string[] fileNames = filePath.Split("\\");
            string fileName = fileNames[fileNames.Length - 1];

            string mediaDurationString = ADMPUtils.GetMediaDurationString(media.Duration);

            mainWindow.PlayPauseButtonImage.Source = new BitmapImage(new Uri("icons/pause_btn.png", UriKind.Relative)); ;
            mainWindow.TopOverlayFilenameText.Text = fileName;
            mainWindow.TopOverlayDurationText.Text = mediaDurationString;
            mainWindow.isPlaying = true;

            Timer labelsTimer = new Timer(5000);
            labelsTimer.Elapsed += (object? sender, ElapsedEventArgs e) =>
            {
                mainWindow.TopOverlay.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new labelHideDelegate(() =>
                {
                    mainWindow.TopOverlayFilenameText.Visibility = Visibility.Hidden;
                    mainWindow.TopOverlayDurationText.Visibility = Visibility.Hidden;
                }));
            };
            labelsTimer.Start();
        }

        //function has an optional parameter to be called without opening an open file dialog
        public async Task LoadSubtitleFile(string? filePath = null)
        {
            if (mainWindow.currentMedia is null)
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
            mainWindow.currentExternalSubtitles = items;

            //making sure the external subtitles are added BEHIND the embedded ones
            int lastTag = 1;
            if (mainWindow.currentSubtitles.Count > 0)
            {
                Debug.WriteLine("Existing subtitle tracks found, determining last tag...");
                lastTag = mainWindow.currentSubtitles!.MaxBy(t => t.Tag).Tag;
            }
            var subtitleTrack = new MainWindow.SubtitleTrack(lastTag, "external", false)
            {
                SubtitleItems = items
            };
            mainWindow.currentSubtitles.Add(subtitleTrack);
            Debug.WriteLine("current subtitle count: " + mainWindow.currentSubtitles.Count);

            _ = mainWindow.GenerateSubtitleTracks();

            //disabling embedded subtitles
            mainWindow.mainMediaPlayer.SetSpu(-1);

            Timer subUpdateTimer = new(100)
            {
                AutoReset = true
            };
            subUpdateTimer.Elapsed += SubtitleUpdateCall;
            subUpdateTimer.Start();
            mainWindow.activeTimers.Add(subUpdateTimer);
        }
        public void SubtitleUpdateCall(object? sender, ElapsedEventArgs e)
        {
            if (mainWindow.currentMedia != null && mainWindow.isPlaying)
            {
                mainWindow.SubtitleDisplay.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SubUpdateDelegate(SubtitleUpdate));
            }
        }
        public delegate void SubUpdateDelegate();
        private void SubtitleUpdate()
        {

            double position = Convert.ToDouble(mainWindow.mainMediaPlayer.Position);
            long duration = mainWindow.mainMediaPlayer.Media.Duration;

            long actualPosition = Convert.ToInt64(Convert.ToDouble(duration) * position);

            int temp = 0;
            if (mainWindow.currentExternalSubtitles.Count > 0)
            {
                temp = mainWindow.currentExternalSubtitles[0].StartTime;
            }

            foreach (SubtitleItem subtitle in mainWindow.currentExternalSubtitles)
            {
                if (actualPosition >= subtitle.StartTime && actualPosition <= subtitle.EndTime)
                {
                    mainWindow.SubtitleDisplay.Visibility = Visibility.Visible;
                    mainWindow.SubtitleDisplay.Text = string.Join("\n", subtitle.Lines);
                    return;
                }
            }
            mainWindow.SubtitleDisplay.Visibility = Visibility.Hidden;
        }

        delegate void labelHideDelegate();
    }
}
