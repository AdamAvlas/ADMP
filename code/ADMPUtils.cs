using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMP.code
{
    public static class ADMPUtils
    {
        public static string GetMediaDurationString(long mediaDurationSrc, bool longerOverride = false)
        {
            TimeSpan mediaDuration = TimeSpan.FromMilliseconds(mediaDurationSrc);
            string mediaDurationStr;

            string hourString = mediaDuration.Hours.ToString();
            if (hourString.Length == 1)
            {
                hourString = "0" + hourString;
            }
            string minuteString = mediaDuration.Minutes.ToString();
            if (minuteString.Length == 1)
            {
                minuteString = "0" + minuteString;
            }
            string secondsString = mediaDuration.Seconds.ToString();
            if (secondsString.Length == 1)
            {
                secondsString = "0" + secondsString;
            }

            if (mediaDuration.Hours > 0 || longerOverride)
            {
                mediaDurationStr = $"{hourString}:{minuteString}:{secondsString}";
            }
            else
            {
                mediaDurationStr = $"{minuteString}:{secondsString}";
            }

            return mediaDurationStr;
        }
    }
}
