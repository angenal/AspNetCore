using System;
using System.Threading.Tasks;

namespace WebSwaggerDemo.NET5.Common
{
    public static class Time
    {
        /// <summary>
        /// Convert To Date String
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToDateString(this DateTime dt, string format = "yyyy-MM-dd")
        {
            return dt.ToString(format);
        }

        /// <summary>
        /// Convert To Time String
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToTimeString(this DateTime dt, string format = "HH:mm:ss")
        {
            return dt.ToString(format);
        }

        /// <summary>
        /// Convert To Time String Equals 00:00:00
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool IsZeroTime(this DateTime dt)
        {
            return dt.ToTimeString().Equals("00:00:00");
        }

        /// <summary>
        /// Convert To DateTime String
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToDateTimeString(this DateTime dt, string format = "yyyy-MM-dd HH:mm:ss")
        {
            return dt.ToString(format);
        }

        /// <summary>
        /// 超时任务
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool TaskTimeout(this TimeSpan timeout, Action action)
        {
            var timeOutTask = Task.Delay(timeout);
            var task = Task.WhenAny(timeOutTask, Task.Run(action)).ConfigureAwait(false).GetAwaiter().GetResult();
            return task == timeOutTask;
        }

        //private static readonly DateTime _epoch_utc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime _epoch_local = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, 0), TimeZoneInfo.Local);
        /// <summary>
        /// 时间戳 DateTime 转化十六进制
        /// </summary>
        public static string ToTimestampHex(this DateTime dateTime, bool toSeconds = true, string format = "x8") => (toSeconds ? ToTimestamp(dateTime) / 1000 : ToTimestamp(dateTime)).ToString(format);
        /// <summary>
        /// 时间戳 Seconds 转化十六进制 小写
        /// </summary>
        public static string x8(this DateTime dateTime, bool toSeconds = true) => dateTime.ToTimestampHex(toSeconds, "x8");
        /// <summary>
        /// 时间戳 Seconds 转化十六进制 大写
        /// </summary>
        public static string X8(this DateTime dateTime, bool toSeconds = true) => dateTime.ToTimestampHex(toSeconds, "X8");
        /// <summary>
        /// 时间戳 Seconds 转化时间值
        /// </summary>
        public static DateTime ToDateTime(this long t, bool toSeconds = true) => toSeconds ? _epoch_local.AddSeconds(t) : _epoch_local.AddMilliseconds(t);
        /// <summary>
        /// 时间戳 Milliseconds 即自1970年1月1日UTC以来经过的毫秒数
        /// </summary>
        public static long ToTimestamp(this DateTime dateTime) => (long)Math.Round(((dateTime < _epoch_local ? DateTime.Now : dateTime) - _epoch_local).TotalMilliseconds, MidpointRounding.AwayFromZero);
    }
}
