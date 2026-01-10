using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace ADMP.Handlers
{
    public class BottomBarHandler
    {
        MainWindow MainWindow { get; set; }
        int _originalVolume;
        bool _isMuted = false;

        public BottomBarHandler(MainWindow mainWindow)
        {
            this.MainWindow = mainWindow;
        }

        public void PlayPause()
        {
            if (MainWindow.mainMediaPlayer.Media is not null)
            {
                if (MainWindow.mainMediaPlayer.IsPlaying)
                {
                    MainWindow.mainMediaPlayer.Pause();
                    MainWindow.PlayPauseButtonImage.Source = new BitmapImage(new Uri("icons/play_btn.png", UriKind.Relative)); ;
                    MainWindow.isPlaying = false;
                    foreach (System.Timers.Timer timer in MainWindow.activeTimers)
                    {
                        timer.Stop();
                    }
                }
                else
                {
                    MainWindow.mainMediaPlayer.Play();
                    MainWindow.PlayPauseButtonImage.Source = new BitmapImage(new Uri("icons/pause_btn.png", UriKind.Relative)); ;
                    MainWindow.isPlaying = true;
                    foreach (System.Timers.Timer timer in MainWindow.activeTimers)
                    {
                        timer.Start();
                    }
                }
            }
        }

        public void MuteUnmuteVideoPlayer()
        {
            if (!_isMuted)//muting
            {
                _isMuted = true;
                MainWindow.VolumeSlider.IsEnabled = false;
                _originalVolume = MainWindow.mainMediaPlayer.Volume;
                MainWindow.mainMediaPlayer.Volume = 0;
                MainWindow.MuteButtonImage.Source = new BitmapImage(new Uri("icons/unmute_btn.png", UriKind.Relative));
            }
            else//unmuting
            {
                _isMuted = false;
                MainWindow.VolumeSlider.IsEnabled = true;
                MainWindow.mainMediaPlayer.Volume = _originalVolume;
                MainWindow.MuteButtonImage.Source = new BitmapImage(new Uri("icons/mute_btn.png", UriKind.Relative));

            }
        }
        public void VolumeChanged()
        {
            int sliderVal = Convert.ToInt32(MainWindow.VolumeSlider.Value * 10);
            MainWindow.mainMediaPlayer.Volume = sliderVal;
            Debug.WriteLine("slider val.:" + sliderVal);
        }

        public void MouseWheelVolumeChange(bool positive)
        {
            if (positive)
            {
                MainWindow.VolumeSlider.Value = MainWindow.VolumeSlider.Value + 0.2;
            }
            else
            {
                MainWindow.VolumeSlider.Value = MainWindow.VolumeSlider.Value - 0.2;
            }
        }

        public void SkipForward()
        {
            if (MainWindow.mainMediaPlayer.Media is null)
            {
                return;
            }

            float currentPosition = MainWindow.mainMediaPlayer.Position;
            float newPosition = currentPosition + 10000f / MainWindow.currentMedia!.Duration;//null-forgiving operator cause if media is null, the code doesnt even reach this point
            if (newPosition <= 1f)
            {
                MainWindow.mainMediaPlayer.Position = newPosition;
            }
        }

        public void SkipBackward()
        {
            if (MainWindow.mainMediaPlayer.Media is null)
            {
                return;
            }

            float currentPosition = MainWindow.mainMediaPlayer.Position;
            float newPosition = currentPosition - 10000f / MainWindow.currentMedia!.Duration;
            if (newPosition >= 0f)
            {
                MainWindow.mainMediaPlayer.Position = newPosition;
            }
        }
    }
}
