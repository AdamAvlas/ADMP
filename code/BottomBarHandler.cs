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
