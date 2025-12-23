using ADMP.Utils;
using System;
using System.Diagnostics;

namespace ADMP.code
{
    public class ProgressBarHandler
    {
        MainWindow mainWindow { get; set; }
        bool isDragging = false;
        bool isBeingUpdated = false;

        public ProgressBarHandler(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void UpdateProgressBar()
        {
            double position = Convert.ToDouble(mainWindow.mainMediaPlayer.Position);
            long duration = mainWindow.mainMediaPlayer.Media.Duration;

            long actualPosition = Convert.ToInt64(Convert.ToDouble(duration) * position);

            string timeElapsedString = "";
            if (TimeSpan.FromMilliseconds(duration).Hours > 0)
            {
                timeElapsedString = Utilities.GetMediaDurationString(actualPosition, true);
            }
            else
            {
                timeElapsedString = Utilities.GetMediaDurationString(actualPosition);
            }
            string durationString = Utilities.GetMediaDurationString(duration);

            double sliderPosition = Convert.ToDouble(mainWindow.mainMediaPlayer.Position) * 10;

            if (!isDragging)
            {
                isBeingUpdated = true;
                mainWindow.ProgressBarSlider.Value = sliderPosition;
                isBeingUpdated = false;
            }
            mainWindow.ProgressBarTimer.Text = $"{timeElapsedString}/{durationString}";
        }

        public void SliderDragEnter()
        {
            isDragging = true;
            Debug.WriteLine("Dragging started...");
        }
        public void SliderDragLeave()
        {
            float playerNewPosition = Convert.ToSingle(mainWindow.ProgressBarSlider.Value) / 10;
            mainWindow.mainMediaPlayer.Position = playerNewPosition;
            isDragging = false;
        }
        public void SliderValueChanged()
        {
            if (!isDragging && !isBeingUpdated)
            {
                float playerNewPosition = Convert.ToSingle(mainWindow.ProgressBarSlider.Value) / 10;
                mainWindow.mainMediaPlayer.Position = playerNewPosition;
            }
        }
    }
}
