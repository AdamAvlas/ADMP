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

namespace ADMP
{
    public class TopMenuHandler
    {
        MainWindow mainWindow { get; set; }
        MediaPlayer mediaPlayer { get; set; }

        public TopMenuHandler(MainWindow setMainWindow, MediaPlayer setMediaPlayer)
        {
            mainWindow = setMainWindow;
            mediaPlayer = setMediaPlayer;
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

                    TimeSpan mediaDuration = TimeSpan.FromMilliseconds(media.Duration);
                    string mediaDurationStr = "";
                    if (mediaDuration.Minutes >= 60)
                    {
                        mediaDurationStr = $"{mediaDuration.Hours}:{mediaDuration.Minutes}:{mediaDuration.Seconds}";
                    }
                    else
                    {
                        mediaDurationStr = $"{mediaDuration.Minutes.ToString()}:{mediaDuration.Seconds}";
                    }

                    mainWindow.PlayPauseButtonText.Text = "PAUSE";
                    mainWindow.TopOverlayFilenameText.Text = fileName;
                    mainWindow.TopOverlayDurationText.Text = mediaDurationStr;
                }
            }
            else
            {
                Debug.WriteLine("File opening canceled/was unsuccessful");
            }
        }
    }
}
