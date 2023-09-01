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
using LibVLCSharp;
using LibVLCSharp.Shared;

namespace ADMP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TopMenuOpenFile(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Opening file...");
            //TopMenuHandler.
        }

        private async void PlayTest(object sender, RoutedEventArgs e)
        {
            string testFilePath = "C:\\Users\\adam\\source\\repos\\ADMP\\media\\tsnuz.mp4";

            Debug.WriteLine("Opening file: " + testFilePath);

            try
            {
                using (var libvlc = new LibVLC())
                {
                    //var videoPlayer = MainVideoPlayer.MediaPlayer;
                    MainVideoPlayer.MediaPlayer = new MediaPlayer(libvlc);

                    var testMedia = new Media(libvlc, testFilePath, FromType.FromPath);
                    await testMedia.Parse();

                    Debug.WriteLine("media duration: " + testMedia.Duration);

                    MainVideoPlayer.MediaPlayer.Play(testMedia);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Sorry vole eror: " + ex.Message + " // " + ex.StackTrace);
            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("This button does nothing");
        }
    }
}
