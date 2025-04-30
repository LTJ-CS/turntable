using System;

namespace Sdk.Runtime.Utility
{
    public static class TimeUtil
    {
        //获取Datetime
        public static DateTime GetLocalDateTime(long timeStamp)
        {
            DateTime utcTime = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(timeStamp);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.Local);
            return localTime;
        }

        //获取时间戳
        public static long GetUnixTimeStamp(DateTime dateTime)
        {
            var timeSpan = dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0);
            return (long)timeSpan.TotalSeconds;
        }  
        
        //获取时间戳
        public static long GetUnixMillTimeStamp(DateTime dateTime)
        {
            var timeSpan = dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0);
            return (long)timeSpan.TotalMilliseconds;
        }

        public static string FormatTimeToMonthAndDay(long timeStamp)
        {
            var dateTime = GetLocalDateTime(timeStamp);
            string formattedDate = dateTime.ToString("M月d日");
            return formattedDate;
        }

        public static string FormatTimestamp(long timeStamp, string format)
        {
            var dateTime = GetLocalDateTime(timeStamp);
            string formattedDate = dateTime.ToString(format);
            return formattedDate;
        }

        /// <summary>
        /// 距离零点的时间
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static string FormatZeroClock(long timeStamp)
        {
            var dateTime = GetLocalDateTime(timeStamp);
            return dateTime.Hour < 23 ? $"{23 - dateTime.Hour}时{59 - dateTime.Minute}分" : $"{59 - dateTime.Minute}分{59 - dateTime.Second}秒";
        }

        /// <summary>
        /// 距离零点的时间
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static string FormatTime(long timeStamp)
        {
            int second = (int)(timeStamp % 60);
            timeStamp /= 60;
            int minute = (int)(timeStamp % 60);
            timeStamp /= 60;
            int hour = (int)(timeStamp % 24);
            timeStamp /= 24;
            int day = (int)timeStamp;
            if (day > 0) return $"{day}天{hour}时";
            if (hour > 0) return $"{hour}时{minute}分";
            if (minute > 0) return $"{minute}分{second}秒";
            return $"{second}秒";
        }
    }
}