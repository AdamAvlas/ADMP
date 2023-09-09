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
        int originalVolume;

        public BottomBarHandler(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void PlayPause()
        {
            if (mainWindow.mainMediaPlayer.Media is not null)
            {
                if (mainWindow.mainMediaPlayer.IsPlaying)
                {
                    mainWindow.mainMediaPlayer.Pause();
                    mainWindow.PlayPauseButtonText.Text = "PLAY";
                }
                else
                {
                    mainWindow.mainMediaPlayer.Play();
                    mainWindow.PlayPauseButtonText.Text = "PAUSE";
                }
            }
        }

        public void MuteUnmuteVideoPlayer()
        {
            if (mainWindow.mainMediaPlayer.Volume == 0)
            {
                if (originalVolume != 0)
                {
                    mainWindow.mainMediaPlayer.Volume = originalVolume;
                }
                else
                {
                    mainWindow.mainMediaPlayer.Volume = 50;
                }
                mainWindow.MuteButtonTextBlock.Text = "MUTE";
            }
            else
            {
                originalVolume = mainWindow.mainMediaPlayer.Volume;
                mainWindow.mainMediaPlayer.Volume = 0;
                mainWindow.MuteButtonTextBlock.Text = "UNM";
            }
        }
    }
}
