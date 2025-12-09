using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ADMP.code;
using LibVLCSharp;
using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;

namespace ADMP
{
    public partial class MainWindow : Window
    {
        public BottomBarHandler bottomBarHandler;
        public TopMenuHandler topMenuHandler;
        public ProgressBarHandler progressBarHandler;
        public LibVLC libVLC = new LibVLC();
        public MediaPlayer mainMediaPlayer;
        public bool isPlaying = false;

        public MainWindow()
        {
            InitializeComponent();

            this.MainVideoPlayer.MediaPlayer = new MediaPlayer(libVLC);
            mainMediaPlayer = this.MainVideoPlayer.MediaPlayer;

            bottomBarHandler = new BottomBarHandler(this);
            topMenuHandler = new TopMenuHandler(this, mainMediaPlayer);
            progressBarHandler = new ProgressBarHandler(this);

            //mainMediaPlayer.PositionChanged += new EventHandler<MediaPlayerPositionChangedEventArgs>(ProgressBarSliderUpdate);

            Timer progressUpdateTimer = new Timer(1000);
            progressUpdateTimer.AutoReset = true;
            progressUpdateTimer.Elapsed += ProgressBarSliderUpdate;
            progressUpdateTimer.Start();

            this.VolumeSlider.Value = Convert.ToDouble(mainMediaPlayer.Volume) / 10;
        }
        public async Task GenerateSubtitleTracks()
        {
            await Task.Delay(1000);
            if (mainMediaPlayer.SpuCount > 0)
            {
                SubtitleTrackList.Visibility = Visibility.Visible;
                Debug.WriteLine("Generating subtitle tracks...");
                TrackDescription[] subtitleDescriptionList = mainMediaPlayer.SpuDescription;
                foreach (TrackDescription subtitleTrack in subtitleDescriptionList)
                {
                    MenuItem menuItem = new() { Header = subtitleTrack.Name, Tag = subtitleTrack.Id };
                    menuItem.Click += (s, e) =>
                    {
                        int selectedSpuId = (int)((MenuItem)s).Tag;
                        mainMediaPlayer.SetSpu(selectedSpuId);
                        Debug.WriteLine("Subtitle track changed to: " + selectedSpuId);
                    };
                    SubtitleTrackList.Items.Add(menuItem);
                }
            }
            else
            {
                Debug.WriteLine("No subtitle tracks found for current media");
                SubtitleTrackList.Visibility = Visibility.Collapsed;
            }
        }
        private void TopMenuOpenFile(object sender, RoutedEventArgs e)
        {
            topMenuHandler.OpenFile();
        }
        private void TopMenuOpenTestFile(object sender, RoutedEventArgs e)
        {
            topMenuHandler.OpenTestFile();
        }
        private void TopMenuOpenSettings(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Opening settings...");
        }

        public void PlayPause(object sender, RoutedEventArgs e)
        {
            bottomBarHandler.PlayPause();
        }

        public delegate void PBUpdate();

        public void ProgressBarSliderUpdate(object? sender, ElapsedEventArgs e)
        {
            if (isPlaying)
            {
                //Debug.WriteLine("updating...");
                this.ProgressBar.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new PBUpdate(progressBarHandler.UpdateProgressBar));
            }
        }

        public void AppQuit(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ProgressBarSliderDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            progressBarHandler.SliderDragEnter();
        }

        private void ProgressBarSliderDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            progressBarHandler.SliderDragLeave();
        }

        private void BottomBarMute(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("muting...");
            bottomBarHandler.MuteUnmuteVideoPlayer();
        }

        private void BottomBarVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bottomBarHandler.VolumeChanged();
        }

        private void VolumeSliderMouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                bottomBarHandler.MouseWheelVolumeChange(true);
            }
            else
            {
                bottomBarHandler.MouseWheelVolumeChange(false);
            }
        }

        private void ProgressBarSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            progressBarHandler.SliderValueChanged();
        }

        //private void LoadSubtitleFile(object sender, RoutedEventArgs e)
        //{
        //    topMenuHandler.LoadSubtitleFile();
        //}
    }
}
