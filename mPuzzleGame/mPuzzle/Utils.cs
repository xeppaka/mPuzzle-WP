using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;

namespace mPuzzle
{
    public class Utils
    {
        private static StringBuilder ttime = new StringBuilder();

        public static string formatTime(long mseconds)
        {
            int min;
            int sec;
            int msec;
            int hour;

            hour = (int)(mseconds / 36000);
            min = (int)((mseconds - hour * 36000) / 600);
            sec = (int)((mseconds - hour * 36000 - min * 600) / 10);
            msec = (int)(mseconds - hour * 36000 - min * 600 - sec * 10);

            ttime.Remove(0, ttime.Length);

            if (hour > 0)
            {
                if (hour > 9)
                    ttime.Append(hour).Append(":");
                else
                    ttime.Append("0").Append(hour).Append(":");
            }

            if (min > 9)
                ttime.Append(min).Append(":");
            else
                ttime.Append("0").Append(min).Append(":");

            if (sec > 9)
                ttime.Append(sec).Append(".");
            else
                ttime.Append("0").Append(sec).Append(".");

            //            if (msec > 9)
            ttime.Append(msec);
            //            else
            //                _time += "0" + msec;

            return ttime.ToString();
        }
    }
}
