using ADMP.Utils;
using System;
using System.Diagnostics;

namespace ADMP.Handlers
{
    public class ProgressBarHandler
    {
        MainWindow MainWindow { get; set; }
        bool isDragging = false;
        bool isBeingUpdated = false;

        public ProgressBarHandler(MainWindow mainWindow)
        {
            this.MainWindow = mainWindow;
        }

        public void UpdateProgressBar()
        {
            if (MainWindow.mainMediaPlayer.Media is null)
            {
                return;
            }
            double position = Convert.ToDouble(MainWindow.mainMediaPlayer.Position);
            long duration = MainWindow.mainMediaPlayer.Media.Duration;

            long actualPosition = Convert.ToInt64(Convert.ToDouble(duration) * position);

            string timeElapsedString;
            if (TimeSpan.FromMilliseconds(duration).Hours > 0)
            {
                timeElapsedString = Utilities.GetMediaDurationString(actualPosition, true);
            }
            else
            {
                timeElapsedString = Utilities.GetMediaDurationString(actualPosition);
            }
            string durationString = Utilities.GetMediaDurationString(duration);

            double sliderPosition = Convert.ToDouble(MainWindow.mainMediaPlayer.Position) * 10;

            if (!isDragging)
            {
                isBeingUpdated = true;
                MainWindow.ProgressBarSlider.Value = sliderPosition;
                isBeingUpdated = false;
            }
            MainWindow.ProgressBarTimer.Text = $"{timeElapsedString}/{durationString}";
        }

        public void SliderDragEnter()
        {
            isDragging = true;
            Debug.WriteLine("Dragging started...");
        }
        public void SliderDragLeave()
        {
            float playerNewPosition = Convert.ToSingle(MainWindow.ProgressBarSlider.Value) / 10;
            MainWindow.mainMediaPlayer.Position = playerNewPosition;
            isDragging = false;
        }
        public void SliderValueChanged()
        {
            if (!isDragging && !isBeingUpdated)
            {
                float playerNewPosition = Convert.ToSingle(MainWindow.ProgressBarSlider.Value) / 10;
                MainWindow.mainMediaPlayer.Position = playerNewPosition;
            }
        }
    }
}
