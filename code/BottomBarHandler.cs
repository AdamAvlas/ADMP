using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMP
{
    public class BottomBarHandler
    {
        MainWindow mainWindow { get; set; }
        int originalVolume;
        bool isMuted = false;

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
                    mainWindow.isPlaying = false;
                }
                else
                {
                    mainWindow.mainMediaPlayer.Play();
                    mainWindow.PlayPauseButtonText.Text = "PAUSE";
                    mainWindow.isPlaying = true;
                }
            }
        }

        public void MuteUnmuteVideoPlayer()
        {
            if (!isMuted)
            {
                isMuted = true;
                mainWindow.VolumeSlider.IsEnabled = false;
                originalVolume = mainWindow.mainMediaPlayer.Volume;
                mainWindow.mainMediaPlayer.Volume = 0;
                mainWindow.MuteButtonTextBlock.Text = "UNM";

            }
            else
            {
                isMuted = false;
                mainWindow.VolumeSlider.IsEnabled = true;
                mainWindow.mainMediaPlayer.Volume = originalVolume;
                mainWindow.MuteButtonTextBlock.Text = "MUTE";

            }
        }
        public void VolumeChanged()
        {
            int sliderVal = Convert.ToInt32(mainWindow.VolumeSlider.Value * 10);
            mainWindow.mainMediaPlayer.Volume = sliderVal;
            Debug.WriteLine("slider val.:" + sliderVal);
        }

        public void MouseWheelVolumeChange(bool positive)
        {
            if (positive)
            {
                mainWindow.VolumeSlider.Value = mainWindow.VolumeSlider.Value + 0.2;
            }
            else
            {
                mainWindow.VolumeSlider.Value = mainWindow.VolumeSlider.Value - 0.2;
            }
        }
    }
}
