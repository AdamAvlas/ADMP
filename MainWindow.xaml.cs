using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace ADMP
{
    public partial class MainWindow : Window
    {
        public BottomBarHandler bottomBarHandler;
        public TopMenuHandler topMenuHandler;
        public ProgressBarHandler progressBarHandler;
        public LibVLC libVLC = new LibVLC();
        public MediaPlayer mainMediaPlayer;

        public MainWindow()
        {
            InitializeComponent();

            this.MainVideoPlayer.MediaPlayer = new MediaPlayer(libVLC);
            mainMediaPlayer = this.MainVideoPlayer.MediaPlayer;

            bottomBarHandler = new BottomBarHandler(this);
            topMenuHandler = new TopMenuHandler(this, this.mainMediaPlayer);
            progressBarHandler = new ProgressBarHandler(this);

            mainMediaPlayer.PositionChanged += new EventHandler<MediaPlayerPositionChangedEventArgs>(ProgressBarSliderUpdate);
            ProgressBarSlider.DragEnter += new DragEventHandler(ProgressBarSliderDragEnter);
            ProgressBarSlider.DragLeave += new DragEventHandler(ProgressBarSliderDragLeave);
        }

        private void TopMenuOpenFile(object sender, RoutedEventArgs e)
        {
            topMenuHandler.OpenFile();
        }
        private void TopMenuOpenTestFile(object sender, RoutedEventArgs e)
        {
            topMenuHandler.OpenTestFile();
        }

        private void PlayTest(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(mainMediaPlayer.Position);
        }

        public void PlayPause(object sender, RoutedEventArgs e)
        {
            bottomBarHandler.PlayPause();
        }

        public delegate void PBUpdate();
        public void ProgressBarSliderUpdate(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            this.ProgressBar.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new PBUpdate(progressBarHandler.UpdateProgressBar));
        }
        public void ProgressBarSliderDragEnter(object sender, DragEventArgs e)
        {
            Debug.WriteLine("slider drag entered");
            //progressBarHandler.SliderDragEnter();
        }
        public void ProgressBarSliderDragLeave(object sender, DragEventArgs e)
        {
            Debug.WriteLine("slider drag left");
            //progressBarHandler.SliderDragLeave();
        }

        public void AppQuit(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ProgressBarSliderDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            progressBarHandler.SliderDragEnter();
            Debug.WriteLine("thumb drag entered");
        }

        private void ProgressBarSliderDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            progressBarHandler.SliderDragLeave();
            Debug.WriteLine("thumb drag completed");
        }
    }
}
