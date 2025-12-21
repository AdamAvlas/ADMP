using ADMP.code;
using LibVLCSharp;
using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using SubtitlesParser.Classes;
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

namespace ADMP
{
    public partial class MainWindow : Window
    {
        public BottomBarHandler bottomBarHandler;
        public TopMenuHandler topMenuHandler;
        public ProgressBarHandler progressBarHandler;
        public LibVLC libVLC = new();
        public MediaPlayer mainMediaPlayer;
        public bool isPlaying = false;
        public List<Timer> activeTimers = [];
        public Media? currentMedia = null;
        public List<SubtitleTrack> currentSubtitles = [];
        public int currentSubtitleIndex = 0;
        public List<SubtitleItem> currentExternalSubtitles = [];

        public MainWindow()
        {
            InitializeComponent();

            this.MainVideoPlayer.MediaPlayer = new MediaPlayer(libVLC);
            mainMediaPlayer = this.MainVideoPlayer.MediaPlayer;

            bottomBarHandler = new BottomBarHandler(this);
            topMenuHandler = new TopMenuHandler(this, mainMediaPlayer);
            progressBarHandler = new ProgressBarHandler(this);

            mainMediaPlayer.EndReached += (s, e) =>
            {
                Debug.WriteLine("Media ended.");
                foreach (var timer in activeTimers)
                {
                    timer.Stop();
                }
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    PlayPauseButtonImage.Source = new BitmapImage(new Uri("icons/play_icon.png", UriKind.Relative));
                    isPlaying = false;
                }));
            };

            Timer progressUpdateTimer = new Timer(1000);
            activeTimers.Add(progressUpdateTimer);
            progressUpdateTimer.AutoReset = true;
            progressUpdateTimer.Elapsed += ProgressBarSliderUpdate;
            progressUpdateTimer.Start();

            this.VolumeSlider.Value = Convert.ToDouble(mainMediaPlayer.Volume) / 10;
        }
        public async Task GetEmbeddedSubtitleTracks()
        {
            await Task.Delay(1000);
            if (mainMediaPlayer.SpuCount > 0)
            {
                SubtitleTrackList.Visibility = Visibility.Visible;
                Debug.WriteLine("Generating embedded subtitle tracks...");
                TrackDescription[] subtitleDescriptionList = mainMediaPlayer.SpuDescription;
                foreach (TrackDescription subtitleTrack in subtitleDescriptionList)
                {
                    var subTrack = new SubtitleTrack(subtitleTrack.Id, subtitleTrack.Name, true);
                    currentSubtitles.Add(subTrack);
                }

                _ = GenerateSubtitleTracks();
            }
            else
            {
                Debug.WriteLine("No subtitle tracks found for current media");
                SubtitleTrackList.Visibility = Visibility.Collapsed;
            }
        }
        public async Task GenerateSubtitleTracks()
        {
            if (currentSubtitles.Count == 0)
            {
                return;
            }

            SubtitleTrackList.Visibility = Visibility.Visible;

            //await Task.Delay(100);
            Debug.WriteLine("re/generating subtitle tracks...");
            bool areAllEmbedded = true;
            SubtitleTrackList.Items.Clear();
            foreach (var item in currentSubtitles)
            {
                MenuItem menuItem = new() { Header = item.Name, Tag = item.Tag };
                if (item.IsEmbedded)
                {
                    menuItem.Click += (s, e) =>
                    {
                        currentExternalSubtitles = [];
                        mainMediaPlayer.SetSpu(item.Tag);
                    };
                }
                else
                {
                    menuItem.Click += (s, e) =>
                    {
                        mainMediaPlayer.SetSpu(-1);
                        currentExternalSubtitles = item.SubtitleItems!;
                    };
                    areAllEmbedded = false;
                }
                SubtitleTrackList.Items.Add(menuItem);
            }
            if (areAllEmbedded == false)
            {
                MenuItem menuItem = new() { Header = "Disable subtitles", Tag = 0 };
                menuItem.Click += (s, e) =>
                {
                    DisableSubtitles();
                };
                SubtitleTrackList.Items.Insert(0, menuItem);
            }
        }

        public void DisableSubtitles()
        {
            Debug.WriteLine("Disabling ALL subtitles");
            mainMediaPlayer.SetSpu(-1);
            currentExternalSubtitles = [];
        }

        private void TopMenuOpenFile(object sender, RoutedEventArgs e)
        {
            topMenuHandler.OpenFile();
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
                ProgressBar.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new PBUpdate(progressBarHandler.UpdateProgressBar));
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

        private void LoadSubtitleFile(object sender, RoutedEventArgs e)
        {
            _ = topMenuHandler.LoadSubtitleFile();
        }

        public class SubtitleTrack(int tag, string name, bool isEmbedded)
        {
            public int Tag { get; set; } = tag;
            public string Name { get; set; } = name;
            public bool IsEmbedded { get; set; } = isEmbedded;
            public List<SubtitleItem>? SubtitleItems { get; set; }
    }
}
}
