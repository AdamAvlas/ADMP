using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMP
{
    public class BottomBarHandler
    {
        MainWindow mainWindow { get; set; }
        MediaPlayer? mediaPlayer;

        public BottomBarHandler(MainWindow setWindow)
        {
            mainWindow = setWindow;
            mediaPlayer = mainWindow.MainVideoPlayer.MediaPlayer;
        }

        public void PlayPause()
        {
            //if (mainWindow.MainVideoPlayer.MediaPlayer.Media is not null)
            //{
            //    if (mainWindow.MainVideoPlayer.MediaPlayer.IsPlaying)
            //    {
            //        mainWindow.MainVideoPlayer.MediaPlayer.Pause();
            //        mainWindow.PlayPauseButtonText.Text = "PLAY";
            //    }
            //    else
            //    {
            //        mainWindow.MainVideoPlayer.MediaPlayer.Play();
            //        mainWindow.PlayPauseButtonText.Text = "PAUSE";
            //    }
            //}
            if (mediaPlayer.Media is not null)
            {
                if (mediaPlayer.IsPlaying)
                {
                    mediaPlayer.Pause();
                    mainWindow.PlayPauseButtonText.Text = "PLAY";
                }
                else
                {
                    mediaPlayer.Play();
                    mainWindow.PlayPauseButtonText.Text = "PAUSE";
                }
            }
        }
    }
}
