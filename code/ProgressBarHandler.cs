using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMP.code
{
    public class ProgressBarHandler
    {
        MainWindow mainWindow { get; set; }
        ADMPUtils utils = new ADMPUtils();

        public ProgressBarHandler(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        //public void ProgressBarSliderHandler()
        //{
        //    //mainWindow.MainProgressBar.
        //    //mainWindow.MainVideoPlayer.MediaPlayer.Media.sta
        //}
        public void UpdateProgressBar()
        {
            float position = mainWindow.mainMediaPlayer.Position;
            double position2 = Convert.ToDouble(position);
            long duration = mainWindow.mainMediaPlayer.Media.Duration;
            double duration2 = Convert.ToDouble(duration);

            double actualPosition = duration2 * position2;

            long actualPosition2 = Convert.ToInt64(actualPosition);

            string positionString = utils.GetMediaDurationString(actualPosition2);
            string durationString = utils.GetMediaDurationString(duration);

            //double slider = mainWindow.ProgressBarSlider.Value;
            //Debug.WriteLine(positionString + " // " + durationString);
            //mainWindow.ProgressBarSlider.Value = mainWindow.mainMediaPlayer.Position;
            //mainWindow.ProgressBarTimer.Text = $"{positionString}:{durationString}";
        }
    }
}
