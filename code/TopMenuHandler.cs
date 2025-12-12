using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Diagnostics;
using LibVLCSharp.Shared;
using ADMP.code;
using System.Timers;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Threading;
using SubtitlesParser.Classes;

namespace ADMP
{
    public class TopMenuHandler
    {
        MainWindow mainWindow { get; set; }
        MediaPlayer mediaPlayer { get; set; }
        private Media currentMedia;

        public TopMenuHandler(MainWindow mainWindow, MediaPlayer mediaPlayer)
        {
            this.mainWindow = mainWindow;
            this.mediaPlayer = mediaPlayer;
        }
        public async void OpenFile()
        {
            string? filePath = "";
            Debug.WriteLine("Attempting to open file...");

            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Multiselect = false;
            ofd.DefaultExt = ".mp4";
            ofd.Filter = "Video files|*.mp4;*.mkv";

            bool? result = ofd.ShowDialog();

            if (result == true)
            {
                filePath = ofd.FileName;
                Debug.WriteLine("File (" + filePath + ") opened successfuly!");

                using (var libvlc = mainWindow.libVLC)
                {
                    Media media = new Media(libvlc, filePath, FromType.FromPath);
                    currentMedia = media;

                    await media.Parse();

                    //string subtitlePath = "file:///C:\\Users\\Adam\\Downloads\\2_English(3).srt";

                    mediaPlayer.Play(media);

                    mainWindow.GenerateSubtitleTracks();


                    mediaPlayer.MediaChanged += (s, e) =>
                    {
                        Debug.WriteLine("media changed");
                    };

                    string[] fileNames = filePath.Split("\\");
                    string fileName = fileNames[fileNames.Length - 1];

                    string mediaDurationString = ADMPUtils.GetMediaDurationString(media.Duration);

                    mainWindow.PlayPauseButtonText.Text = "PAUSE";
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
            }
            else
            {
                Debug.WriteLine("File opening canceled/was unsuccessful");
            }
        }

        //public async Task LoadSubtitleFile()
        //{
        //    if (!mediaPlayer.IsPlaying)
        //    {
        //        Debug.WriteLine("Cannot load subtitle file, because no media is playing!");
        //        return;
        //    }
        //    string subtitlePath = "C:\\Users\\Adam\\Downloads\\2_English(3).srt";

        //await Task.Delay(1000);
        //if (File.Exists(subtitlePath))
        //{
        //    string subtitleUri = new Uri(subtitlePath).AbsolutePath;
        //    Debug.WriteLine("Adding subtitle file: " + subtitlePath);
        //bool success = mediaPlayer.AddSlave(MediaSlaveType.Subtitle, subtitleUri, true);
        //bool success = currentMedia.AddSlave(MediaSlaveType.Subtitle, 1, subtitleUri);
        //if (success)
        //{
        //    Debug.WriteLine("Subtitle file added successfully!");
        //}
        //else
        //{
        //    Debug.WriteLine("Failed to add subtitle file.");
        //    return;
        //}
        //media.AddOption(":subsdec-encoding=UTF-8");
        //media.AddOption(":sub-pos=30");
        //media.AddOption(":freetype-rel-fontsize=50");
        //media.AddOption(":freetype-color=16711680");
        //mediaPlayer.SetSpu(-1);

        //mediaPlayer.Stop();
        //await Task.Delay(200);
        //mediaPlayer.Play();
        //mediaPlayer.Stop();
        //Media newMedia = new Media(mainWindow.libVLC, currentMedia.Mrl, FromType.FromLocation);
        //currentMedia.Dispose();
        //await newMedia.Parse();
        //mediaPlayer.Play(newMedia);

        //await Task.Delay(1000);
        //Debug.WriteLine("restarted subtitle count: " + mediaPlayer.SpuCount);
        //mediaPlayer.SetSpu(1);
        //        mainWindow.GenerateSubtitleTracks();
        //    }
        //    else
        //    {
        //        Debug.WriteLine("Subtitle file not found!");
        //    }
        //}
        public async void LoadSubtitleFile()
        {
            if (!mediaPlayer.IsPlaying)
            {
                Debug.WriteLine("Cannot load subtitle file, because no media is playing!");
                return;
            }
            //OpenFileDialog ofd = new OpenFileDialog();
            //ofd.Multiselect = false;
            //ofd.DefaultExt = ".srt";
            //ofd.Filter = "Subtitle files|*.srt;*.sub;*.";
            string subtitlePath = "C:\\Users\\adama\\Downloads\\2_English(3).srt";
            if (!File.Exists(subtitlePath))
            {
                Debug.WriteLine("Subtitle file not found!");
                return;
            }
            var parser = new SubtitlesParser.Classes.Parsers.SubParser();

            using (var fileStream = File.OpenRead(subtitlePath))
            {
                try
                {
                    var mostLikelyFormat = parser.GetMostLikelyFormat(subtitlePath);
                    Debug.WriteLine("Most likely subtitle format: " + mostLikelyFormat.Name);
                    var items = parser.ParseStream(fileStream, Encoding.UTF8, mostLikelyFormat);
                    Debug.WriteLine("Parsed " + items.Count + " subtitle items.");

                    Debug.WriteLine($"Line: {items[0].Lines[0]};ST: {items[0].StartTime};ET: {items[0].EndTime}");

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error parsing subtitle file: " + ex.Message);
                }
            }
        }
        private async void SubtitleDisplay()
        {
            int subIndex = 0;
            while (mediaPlayer.IsPlaying)
            {
                //foreach (var subtitle in subtitles)
                //{
                //    if (currentTime >= subtitle.StartTime && currentTime <= subtitle.EndTime)
                //    {
                //        // Display subtitle
                //        mainWindow.SubtitleTextBlock.Text = string.Join("\n", subtitle.Lines);
                //    }
                //}
                //await Task.Delay(500); // Check every 500 milliseconds
            }
        }

        delegate void labelHideDelegate();

        public async void OpenTestFile()
        {
            string filePath = "C:\\Users\\adam\\source\\repos\\ADMP\\media\\test_files\\u_intro.mp4";
            Debug.WriteLine("File (" + filePath + ") opened successfuly!");

            using (var libvlc = new LibVLC())
            {
                Media media = new Media(libvlc, filePath, FromType.FromPath);
                await media.Parse();

                mediaPlayer.Play(media);

                string[] fileNames = filePath.Split("\\");
                string fileName = fileNames[fileNames.Length - 1];

                string mediaDurationString = ADMPUtils.GetMediaDurationString(media.Duration);

                mainWindow.PlayPauseButtonText.Text = "PAUSE";
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
        }
    }
}
