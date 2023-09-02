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
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ADMP.code;
using LibVLCSharp;
using LibVLCSharp.Shared;

namespace ADMP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

            //PreviousButton.Click += new RoutedEventHandler(ProgressBarSliderUpdate);
            mainMediaPlayer.PositionChanged += new EventHandler<MediaPlayerPositionChangedEventArgs>(ProgressBarSliderUpdate);
        }

        private void TopMenuOpenFile(object sender, RoutedEventArgs e)
        {
            topMenuHandler.OpenFile();
        }

        private void PlayTest(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(mainMediaPlayer.Position);
        }

        public void PlayPause(object sender, RoutedEventArgs e)
        {
            bottomBarHandler.PlayPause();
        }

        public void ProgressBarSliderUpdate(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            progressBarHandler.UpdateProgressBar();
        }

        public void AppQuit(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
