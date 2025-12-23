using ADMP.code;
using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ADMP
{
    public partial class MainWindow : Window
    {
        public BottomBarHandler bottomBarHandler;
        public TopMenuHandler topMenuHandler;
        public ProgressBarHandler progressBarHandler;
        public SettingsHandler settingsHandler;
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
            settingsHandler = new SettingsHandler();

            settingsHandler.LoadSettings();//loading settings
            if (settingsHandler.AppSettings.VolumeLevel is not null)
            {
                mainMediaPlayer.Volume = (int)settingsHandler.AppSettings.VolumeLevel!;
                VolumeSlider.Value = Convert.ToDouble(mainMediaPlayer.Volume) / 10;

            }
            else
            {
                mainMediaPlayer.Volume = 50;
                VolumeSlider.Value = 5;
            }
            if (settingsHandler.AppSettings.LastOpened.Count > 0)
            {
                Debug.WriteLine("Generating recent files list...");
                RecentlyOpenedList.Visibility = Visibility.Visible;
                int tag = 1;
                foreach (string item in settingsHandler.AppSettings.LastOpened)
                {
                    MenuItem menuItem = new() { Header = item, Tag = tag };
                    menuItem.Click += (s, e) =>
                    {
                        Debug.WriteLine("Opening recent file: " + item);
                        topMenuHandler.OpenFile(item);
                    };

                    RecentlyOpenedList.Items.Insert(0, menuItem);
                    tag++;
                }
            }

            Timer progressUpdateTimer = new(1000);
            activeTimers.Add(progressUpdateTimer);
            progressUpdateTimer.AutoReset = true;
            progressUpdateTimer.Elapsed += ProgressBarSliderUpdate;
            progressUpdateTimer.Start();

            this.VolumeSlider.Value = Convert.ToDouble(mainMediaPlayer.Volume) / 10;

            mainMediaPlayer.EndReached += (s, e) =>
            {
                Debug.WriteLine("Media ended.");
                foreach (Timer timer in activeTimers)
                {
                    timer.Stop();
                }
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    PlayPauseButtonImage.Source = new BitmapImage(new Uri("icons/play_icon.png", UriKind.Relative));
                    isPlaying = false;
                }));
            };
            Closing += (s, e) =>
            {
                AppQuit(s, e);
            };
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
                    SubtitleTrack subTrack = new(subtitleTrack.Id, subtitleTrack.Name, true);
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
            foreach (SubtitleTrack item in currentSubtitles)
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
            settingsHandler.AppSettings.VolumeLevel = mainMediaPlayer.Volume;
            settingsHandler.SaveSettings();
            Environment.Exit(0);
        }
        public void AppQuit(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            settingsHandler.AppSettings.VolumeLevel = mainMediaPlayer.Volume;
            settingsHandler.SaveSettings();
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

        public void SkipForward(object sender, RoutedEventArgs e)
        {
            bottomBarHandler.SkipForward();
        }

        public void SkipBackward(object sender, RoutedEventArgs e)
        {
            bottomBarHandler.SkipBackward();
        }

        public void ClearRecentList(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Clearing recent files list...");

            if (RecentlyOpenedList.Items.Count <= 1)
            {
                return;
            }

            settingsHandler.AppSettings.LastOpened.Clear();
            List<MenuItem> itemsToRemove = [];
            foreach (object? item in RecentlyOpenedList.Items)
            {
                if (item is not MenuItem)//cause theres a separator in there,and it WILL crash badly if this isnt here
                {
                    continue;
                }
                MenuItem menuItem = (MenuItem)item;
                if ((string)menuItem.Header != "Clear recent")//to make sure this doesnt remove the "Clear recent" button itself
                {
                    itemsToRemove.Add(menuItem);
                }
            }
            foreach (MenuItem item in itemsToRemove)//note: two loops required,cause you cant remove items from a collection youre iterating through
            {
                RecentlyOpenedList.Items.Remove(item);
            }

            RecentlyOpenedList.Items.Clear();
            RecentlyOpenedList.Visibility = Visibility.Collapsed;
        }

        public void AddToRecentFilesList(string filePath)
        {
            Debug.WriteLine("Adding to main window recent files list: " + filePath);

            if (RecentlyOpenedList.Items.Count >= 6)
            {
                Debug.WriteLine("Removing excess items...");
                RecentlyOpenedList.Items.RemoveAt(4);
            }
            int lastTag = RecentlyOpenedList.Items
                .OfType<MenuItem>()
                .Select(mi => mi.Tag is int tag ? tag : 0)
                .DefaultIfEmpty(0)
                .Max();//linq for getting the largest, and therefore last tag
            MenuItem menuItem = new() { Header = filePath, Tag = lastTag + 1 };
            menuItem.Click += (s, e) =>
            {
                Debug.WriteLine("Opening recent file: " + filePath);
                topMenuHandler.OpenFile(filePath);
            };
            RecentlyOpenedList.Items.Insert(0, menuItem);
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
