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

namespace ADMP
{
    public class TopMenuHandler
    {
        MainWindow mainWindow { get; set; }
        MediaPlayer mediaPlayer { get; set; }

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
            ofd.Filter = "Video files|*.mp4";

            bool? result = ofd.ShowDialog();

            if (result == true)
            {
                filePath = ofd.FileName;
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
            else
            {
                Debug.WriteLine("File opening canceled/was unsuccessful");
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
