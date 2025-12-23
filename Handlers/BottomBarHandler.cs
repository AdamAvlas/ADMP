using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;

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
                    mainWindow.PlayPauseButtonImage.Source = new BitmapImage(new Uri("icons/play_btn.png", UriKind.Relative)); ;
                    mainWindow.isPlaying = false;
                    foreach (var timer in mainWindow.activeTimers)
                    {
                        timer.Stop();
                    }
                }
                else
                {
                    mainWindow.mainMediaPlayer.Play();
                    mainWindow.PlayPauseButtonImage.Source = new BitmapImage(new Uri("icons/pause_btn.png", UriKind.Relative)); ;
                    mainWindow.isPlaying = true;
                    foreach (var timer in mainWindow.activeTimers)
                    {
                        timer.Start();
                    }
                }
            }
        }

        public void MuteUnmuteVideoPlayer()
        {
            if (!isMuted)//muting
            {
                isMuted = true;
                mainWindow.VolumeSlider.IsEnabled = false;
                originalVolume = mainWindow.mainMediaPlayer.Volume;
                mainWindow.mainMediaPlayer.Volume = 0;
                mainWindow.MuteButtonImage.Source = new BitmapImage(new Uri("icons/unmute_btn.png", UriKind.Relative));
            }
            else//unmuting
            {
                isMuted = false;
                mainWindow.VolumeSlider.IsEnabled = true;
                mainWindow.mainMediaPlayer.Volume = originalVolume;
                mainWindow.MuteButtonImage.Source = new BitmapImage(new Uri("icons/mute_btn.png", UriKind.Relative));

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

        public void SkipForward()
        {
            if (mainWindow.mainMediaPlayer.Media is null)
            {
                return;
            }

            float currentPosition = mainWindow.mainMediaPlayer.Position;
            float newPosition = currentPosition + (10000f / mainWindow.currentMedia.Duration);
            if (newPosition <= 1f)
            {
                mainWindow.mainMediaPlayer.Position = newPosition;
            }
        }

        public void SkipBackward()
        {
            if (mainWindow.mainMediaPlayer.Media is null)
            {
                return;
            }

            float currentPosition = mainWindow.mainMediaPlayer.Position;
            float newPosition = currentPosition - (10000f / mainWindow.currentMedia.Duration);
            if (newPosition >= 0f)
            {
                mainWindow.mainMediaPlayer.Position = newPosition;
            }
        }
    }
}
