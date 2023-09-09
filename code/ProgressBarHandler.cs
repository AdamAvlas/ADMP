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
        bool isDragging = false;

        public ProgressBarHandler(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void UpdateProgressBar()
        {
            double position = Convert.ToDouble(mainWindow.mainMediaPlayer.Position);
            long duration = mainWindow.mainMediaPlayer.Media.Duration;

            long actualPosition = Convert.ToInt64(Convert.ToDouble(duration) * position);

            string positionString = "";
            if (TimeSpan.FromMilliseconds(duration).Hours > 0)
            {
                positionString = ADMPUtils.GetMediaDurationString(actualPosition, true);
            }
            else
            {
                positionString = ADMPUtils.GetMediaDurationString(actualPosition);
            }
            string durationString = ADMPUtils.GetMediaDurationString(duration);

            double sliderPosition = Convert.ToDouble(mainWindow.mainMediaPlayer.Position) * 10;

            if (!isDragging)
            {
                mainWindow.ProgressBarSlider.Value = sliderPosition;
            }
            mainWindow.ProgressBarTimer.Text = $"{positionString}/{durationString}";
        }

        public void SliderDragEnter()
        {
            isDragging = true;
        }
        public void SliderDragLeave()
        {
            float playerNewPosition = Convert.ToSingle(mainWindow.ProgressBarSlider.Value) / 10;
            mainWindow.mainMediaPlayer.Position = playerNewPosition;
            isDragging = false;
        }
    }
}
