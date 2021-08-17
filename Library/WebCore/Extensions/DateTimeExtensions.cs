using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WebCore.Json;

namespace WebCore
{
    /// <summary>
    /// DateTime Extensions JavaScript
    /// </summary>
    public static class DateTimeExtensions
    {
        #region private fields and methods

        private static readonly DateTime _epoch_utc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime _epoch_local = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, 0), TimeZoneInfo.Local);

        /// <summary>
        /// new DateTime(1970, 1, 1).Ticks
        /// </summary>
        private const long InitialJavaScriptDateTicks = 621355968000000000;

        // Number of 100ns ticks per time unit
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;

        // Number of milliseconds per time unit
        private const int MillisPerSecond = 1000;
        private const int MillisPerMinute = MillisPerSecond * 60;
        private const int MillisPerHour = MillisPerMinute * 60;
        private const int MillisPerDay = MillisPerHour * 24;

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097

        // Number of days from 1/1/0001 to 12/31/1600
        private const int DaysTo1601 = DaysPer400Years * 4;          // 584388
        // Number of days from 1/1/0001 to 12/30/1899
        private const int DaysTo1899 = DaysPer400Years * 4 + DaysPer100Years * 3 - 367;
        // Number of days from 1/1/0001 to 12/31/9999
        private const int DaysTo10000 = DaysPer400Years * 25 - 366;  // 3652059

        internal const long MinTicks = 0;
        internal const long MaxTicks = DaysTo10000 * TicksPerDay - 1;

        private static readonly int[] DaysToMonth365 = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
        private static readonly int[] DaysToMonth366 = { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };

        private static readonly char[][] _fourDigits = CreateFourDigitsCache();
        private static readonly byte[][] _fourDigitsByte = CreateFourDigitsByteCache();

        private static char[][] CreateFourDigitsCache()
        {
            var c = new char[10000][];
            for (int i = 0; i < 10000; i++)
            {
                c[i] = new[]
                {
                    (char) (i/1000 + '0'),
                    (char) ((i%1000)/100 + '0'),
                    (char) ((i%100)/10 + '0'),
                    (char) (i%10 + '0')
                };
            }
            return c;
        }

        private static byte[][] CreateFourDigitsByteCache()
        {
            var c = new byte[10000][];
            for (int i = 0; i < 10000; i++)
            {
                c[i] = new[]
                {
                    (byte) (i/1000 + '0'),
                    (byte) ((i%1000)/100 + '0'),
                    (byte) ((i%100)/10 + '0'),
                    (byte) (i%10 + '0')
                };
            }
            return c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void ProcessDefaultFormat(long ticks, char* chars)
        {
            // n = number of days since 1/1/0001
            int n = (int)(ticks / TicksPerDay);
            // y400 = number of whole 400-year periods since 1/1/0001
            int y400 = n / DaysPer400Years;
            // n = day number within 400-year period
            n -= y400 * DaysPer400Years;
            // y100 = number of whole 100-year periods within 400-year period
            int y100 = n / DaysPer100Years;
            // Last 100-year period has an extra day, so decrement result if 4
            if (y100 == 4) y100 = 3;
            // n = day number within 100-year period
            n -= y100 * DaysPer100Years;
            // y4 = number of whole 4-year periods within 100-year period
            int y4 = n / DaysPer4Years;
            // n = day number within 4-year period
            n -= y4 * DaysPer4Years;
            // y1 = number of whole years within 4-year period
            int y1 = n / DaysPerYear;
            // Last year has an extra day, so decrement result if 4
            if (y1 == 4) y1 = 3;
            // If year was requested, compute and return it
            var year = y400 * 400 + y100 * 100 + y4 * 4 + y1 + 1;

            // n = day number within year
            n -= y1 * DaysPerYear;
            // Leap year calculation looks different from IsLeapYear since y1, y4,
            // and y100 are relative to year 1, not year 0
            bool leapYear = y1 == 3 && (y4 != 24 || y100 == 3);
            int[] days = leapYear ? DaysToMonth366 : DaysToMonth365;
            // All months have less than 32 days, so n >> 5 is a good conservative
            // estimate for the month
            int month = n >> 5 + 1;
            // m = 1-based month number
            while (n >= days[month]) month++;
            // If month was requested, return it

            // Return 1-based day-of-month
            var day = n - days[month - 1] + 1;

            var v = _fourDigits[year];
            chars[0] = v[0];
            chars[1] = v[1];
            chars[2] = v[2];
            chars[3] = v[3];
            chars[4] = '-';
            v = _fourDigits[month];
            chars[5] = v[2];
            chars[5 + 1] = v[3];
            chars[7] = '-';
            v = _fourDigits[day];
            chars[8] = v[2];
            chars[8 + 1] = v[3];
            chars[10] = 'T';
            v = _fourDigits[(ticks / TicksPerHour) % 24];
            chars[11] = v[2];
            chars[11 + 1] = v[3];
            chars[13] = ':';
            v = _fourDigits[(ticks / TicksPerMinute) % 60];
            chars[14] = v[2];
            chars[14 + 1] = v[3];
            chars[16] = ':';
            v = _fourDigits[(ticks / TicksPerSecond) % 60];
            chars[17] = v[2];
            chars[17 + 1] = v[3];
            chars[19] = '.';

            long fraction = (ticks % 10000000);
            v = _fourDigits[fraction / 10000];
            chars[20] = v[1];
            chars[21] = v[2];
            chars[22] = v[3];

            fraction = fraction % 10000;

            v = _fourDigits[fraction];
            chars[23] = v[0];
            chars[24] = v[1];
            chars[25] = v[2];
            chars[26] = v[3];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void ProcessDefaultFormat(long ticks, byte* chars)
        {
            // n = number of days since 1/1/0001
            int n = (int)(ticks / TicksPerDay);
            // y400 = number of whole 400-year periods since 1/1/0001
            int y400 = n / DaysPer400Years;
            // n = day number within 400-year period
            n -= y400 * DaysPer400Years;
            // y100 = number of whole 100-year periods within 400-year period
            int y100 = n / DaysPer100Years;
            // Last 100-year period has an extra day, so decrement result if 4
            if (y100 == 4) y100 = 3;
            // n = day number within 100-year period
            n -= y100 * DaysPer100Years;
            // y4 = number of whole 4-year periods within 100-year period
            int y4 = n / DaysPer4Years;
            // n = day number within 4-year period
            n -= y4 * DaysPer4Years;
            // y1 = number of whole years within 4-year period
            int y1 = n / DaysPerYear;
            // Last year has an extra day, so decrement result if 4
            if (y1 == 4) y1 = 3;
            // If year was requested, compute and return it
            var year = y400 * 400 + y100 * 100 + y4 * 4 + y1 + 1;

            // n = day number within year
            n -= y1 * DaysPerYear;
            // Leap year calculation looks different from IsLeapYear since y1, y4,
            // and y100 are relative to year 1, not year 0
            bool leapYear = y1 == 3 && (y4 != 24 || y100 == 3);
            int[] days = leapYear ? DaysToMonth366 : DaysToMonth365;
            // All months have less than 32 days, so n >> 5 is a good conservative
            // estimate for the month
            int month = n >> 5 + 1;
            // m = 1-based month number
            while (n >= days[month]) month++;
            // If month was requested, return it

            // Return 1-based day-of-month
            var day = n - days[month - 1] + 1;

            var v = _fourDigitsByte[year];
            chars[0] = v[0];
            chars[1] = v[1];
            chars[2] = v[2];
            chars[3] = v[3];
            chars[4] = (byte)'-';
            v = _fourDigitsByte[month];
            chars[5] = v[2];
            chars[5 + 1] = v[3];
            chars[7] = (byte)'-';
            v = _fourDigitsByte[day];
            chars[8] = v[2];
            chars[8 + 1] = v[3];
            chars[10] = (byte)'T';
            v = _fourDigitsByte[(ticks / TicksPerHour) % 24];
            chars[11] = v[2];
            chars[11 + 1] = v[3];
            chars[13] = (byte)':';
            v = _fourDigitsByte[(ticks / TicksPerMinute) % 60];
            chars[14] = v[2];
            chars[14 + 1] = v[3];
            chars[16] = (byte)':';
            v = _fourDigitsByte[(ticks / TicksPerSecond) % 60];
            chars[17] = v[2];
            chars[17 + 1] = v[3];
            chars[19] = (byte)'.';

            long fraction = (ticks % 10000000);
            v = _fourDigitsByte[fraction / 10000];
            chars[20] = v[1];
            chars[21] = v[2];
            chars[22] = v[3];

            fraction = fraction % 10000;

            v = _fourDigitsByte[fraction];
            chars[23] = v[0];
            chars[24] = v[1];
            chars[25] = v[2];
            chars[26] = v[3];
        }

        private static int ThrowMemoryIsNotBigEnough()
        {
            throw new ArgumentException($"The memory passed to {nameof(GetDefaultFormat)} is not big enough, we require at least 28 bytes to operate. This exception should never ever happen.");
        }

        #endregion


        /// <summary>
        /// This function Processes the to string format of the form "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff" for date times in
        /// invariant culture scenarios. This implementation takes 20% of the time of a regular .ToString(format) call
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="isUtc"></param>
        /// <returns></returns>
        public static unsafe string GetDefaultFormat(this DateTime dt, bool isUtc = false)
        {
            string result = new string('Z', 27 + (isUtc ? 1 : 0));

            var ticks = dt.Ticks;

            fixed (char* chars = result)
            {
                ProcessDefaultFormat(ticks, chars);
            }

            return result;
        }

        /// <summary>
        /// This function Processes the to string format of the form "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff" for date times in
        /// invariant culture scenarios. This implementation takes 20% of the time of a regular .ToString(format) call
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="isUtc"></param>
        /// <returns></returns>
        public static unsafe ByteStringContext.InternalScope GetDefaultFormat(this DateTime dt, ByteStringContext context, out ByteString value, bool isUtc = false)
        {
            int size = 27 + (isUtc ? 1 : 0);
            var ticks = dt.Ticks;

            var scope = context.Allocate(size, out value);

            byte* ptr = value.Ptr;
            ProcessDefaultFormat(ticks, ptr);
            ptr[size - 1] = (byte)'Z';

            return scope;
        }

        /// <summary>
        /// This function Processes the to string format of the form "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff" for date times in
        /// invariant culture scenarios. This implementation takes 20% of the time of a regular .ToString(format) call
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="context"></param>
        /// <param name="memory"></param>
        /// <param name="isUtc"></param>
        /// <returns></returns>
        public static unsafe int GetDefaultFormat(this DateTime dt, JsonOperationContext context, out AllocatedMemoryData memory, bool isUtc = false)
        {
            int size = 27 + (isUtc ? 1 : 0);
            var ticks = dt.Ticks;

            memory = context.GetMemory(size);

            byte* ptr = memory.Address;
            ProcessDefaultFormat(ticks, ptr);

            if (isUtc)
                ptr[size - 1] = (byte)'Z';

            return size;
        }


        /// <summary>
        /// This function Processes the to string format of the form "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff" for date times in
        /// invariant culture scenarios. This implementation takes 20% of the time of a regular .ToString(format) call
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="memory"></param>
        /// <param name="isUtc"></param>
        /// <returns></returns>
        public static unsafe int GetDefaultFormat(this DateTime dt, AllocatedMemoryData memory, bool isUtc = false)
        {
            int size = 27 + (isUtc ? 1 : 0);
            if (memory.SizeInBytes < size)
                goto Error;

            var ticks = dt.Ticks;

            byte* ptr = memory.Address;
            ProcessDefaultFormat(ticks, ptr);

            if (isUtc)
                ptr[size - 1] = (byte)'Z';

            return size;

        Error:
            return ThrowMemoryIsNotBigEnough();
        }

        /// <summary>
        /// ParseDateMicrosoft ConvertJavaScriptTicksToDateTime
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static DateTime ParseDateMicrosoft(this string text, DateTimeKind kind = DateTimeKind.Utc)
        {
            var value = text.Substring(6, text.Length - 8);

            var index = value.IndexOf('+', 1);

            if (index == -1)
                index = value.IndexOf('-', 1);

            if (index != -1)
                value = value.Substring(0, index);

            var javaScriptTicks = long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

            return FromJavaScriptTicks(javaScriptTicks, kind);
        }
        /// <summary>
        /// 转换JavaScript时间戳为Utc时间
        /// </summary>
        /// <param name="javaScriptTicks"></param>
        /// <param name="kind">Utc</param>
        /// <returns></returns>
        public static DateTime FromJavaScriptTicks(this long javaScriptTicks, DateTimeKind kind = DateTimeKind.Utc)
        {
            return new DateTime((javaScriptTicks * 10000) + InitialJavaScriptDateTicks, kind);
        }
        /// <summary>
        /// 转换Utc时间为JavaScript时间戳
        /// </summary>
        /// <param name="dateTime">DateTime.UtcNow</param>
        /// <returns></returns>
        public static long ToJavaScriptTicks(this DateTime dateTime)
        {
            return ((dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime()).Ticks - InitialJavaScriptDateTicks) / 10000;
        }





        /// <summary>Converts a unix timestamp to a DateTime. </summary>
        /// <param name="unixTimeStamp">The unix timestamp. </param>
        /// <param name="kind">The kind of the unit timestamp and return value. </param>
        /// <returns>The date time. </returns>
        public static DateTime FromUnixTimeStamp(this double unixTimeStamp, DateTimeKind kind = DateTimeKind.Utc)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, kind);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

        /// <summary>Converts a DateTime to an unix timestamp. </summary>
        /// <param name="dateTime">The date time. </param>
        /// <param name="kind">The kind of the date time and return value. </param>
        /// <returns>The unix timestamp. </returns>
        public static double ToUnixTimeStamp(this DateTime dateTime, DateTimeKind kind = DateTimeKind.Utc)
        {
            dateTime = kind == DateTimeKind.Local ? dateTime.ToLocalTime() : dateTime.ToUniversalTime();
            return (dateTime - new DateTime(1970, 1, 1, 0, 0, 0, kind)).TotalSeconds;
        }

        /// <summary>Changes only the time part of the DateTime. </summary>
        /// <param name="date">The date. </param>
        /// <param name="hour">The hour. </param>
        /// <param name="minute">The minute. </param>
        /// <param name="second">The second. </param>
        /// <param name="millisecond"></param>
        /// <returns></returns>
        public static DateTime SetTimeTakeDate(this DateTime date, int hour, int minute, int second, int millisecond = 0)
        {
            return new DateTime(date.Year, date.Month, date.Day, hour, minute, second, millisecond);
        }
        /// <summary>
        /// Changes only the time part of the DateTime.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="millisecond"></param>
        /// <returns></returns>
        public static DateTime SetTimeTakeDate(this DateTime date, int day, int hour, int minute, int second, int millisecond = 0)
        {
            return new DateTime(date.Year, date.Month, day, hour, minute, second, millisecond);
        }





        /// <summary>Resets the time part to 00:00:00. </summary>
        /// <param name="dt">The date time to work with. </param>
        /// <returns>The new date time. </returns>
        public static DateTime ToStartOfDay(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
        }

        /// <summary>Sets the time part to the latest time of the day. </summary>
        /// <param name="dt">The date time to work with. </param>
        /// <returns>The new date time. </returns>
        public static DateTime ToEndOfDay(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 0);
        }

        /// <summary>Resets the time part to 00:00:00. </summary>
        /// <param name="dt">The date time to work with. </param>
        /// <returns>The new date time. </returns>
        public static DateTime? ToStartOfDay(this DateTime? dt)
        {
            return dt.HasValue ? dt.Value.ToStartOfDay() : (DateTime?)null;
        }

        /// <summary>Sets the time part to the latest time of the day. </summary>
        /// <param name="dt">The date time to work with. </param>
        /// <returns>The new date time. </returns>
        public static DateTime? ToEndOfDay(this DateTime? dt)
        {
            return dt.HasValue ? dt.Value.ToEndOfDay() : (DateTime?)null;
        }



        /// <summary>
        /// Sets the time part to the latest time of this week.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime ToEndOfWeek(this DateTime dt)
        {
            return dt.AddDays(7 - dt.DayOfWeek.GetHashCode()).ToEndOfDay();
        }
        /// <summary>
        /// Sets the time part to the latest time of this month.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime ToEndOfMonth(this DateTime dt)
        {
            return dt.AddMonths(1).SetTimeTakeDate(1, 0, 0, 0).AddMilliseconds(-1);
        }
        /// <summary>
        /// Sets the time part to the latest time of this year.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime ToEndOfYear(this DateTime dt)
        {
            return new DateTime(dt.Year + 1, 12, 31, 23, 59, 59, 999);
        }



        /// <summary>Checks whether a date time is between two date times. </summary>
        /// <param name="dt">The date time to work with. </param>
        /// <param name="start">The starting date time. </param>
        /// <param name="end">The ending start time. </param>
        /// <returns>True when the date time is between. </returns>
        public static bool IsBetween(this DateTime dt, DateTime start, DateTime end)
        {
            return start <= dt && dt < end;
        }

        /// <summary>Checks whether a date time is between two date times. </summary>
        /// <param name="dt">The date time to work with. </param>
        /// <param name="start">The starting date time. </param>
        /// <param name="end">The ending start time. </param>
        /// <returns>True when the date time is between. </returns>
        public static bool IsBetween(this DateTime? dt, DateTime start, DateTime end)
        {
            return dt.HasValue && dt.Value.IsBetween(start, end);
        }

        /// <summary>Checks whether a date time is between two date times. </summary>
        /// <param name="dt">The date time to work with. </param>
        /// <param name="start">The starting date time. </param>
        /// <param name="end">The ending start time. Null means undefinitely in the future. </param>
        /// <returns>True when the date time is between. </returns>
        public static bool IsBetween(this DateTime dt, DateTime start, DateTime? end)
        {
            return start <= dt && (end == null || dt < end.Value);
        }

        /// <summary>Checks whether a date time is between two date times. </summary>
        /// <param name="dt">The date time to work with. </param>
        /// <param name="start">The starting date time. </param>
        /// <param name="end">The ending start time. Null means undefinitely in the future. </param>
        /// <returns>True when the date time is between. </returns>
        public static bool IsBetween(this DateTime? dt, DateTime start, DateTime? end)
        {
            return dt.HasValue && dt.Value.IsBetween(start, end);
        }

        /// <summary>
        /// Checks whether a date time is between two date times.
        /// </summary>
        /// <param name="dt">The date time to work with. </param>
        /// <param name="start">The starting date time. Null means undefinitely in the past. </param>
        /// <param name="end">The ending start time. Null means undefinitely in the future. </param>
        /// <returns>True when the date time is between. </returns>
        public static bool IsBetween(this DateTime dt, DateTime? start, DateTime? end)
        {
            return (start == null || start.Value <= dt) && (end == null || dt < end.Value);
        }

        /// <summary>Checks whether a date time is between two date times. </summary>
        /// <param name="dt">The date time to work with. </param>
        /// <param name="start">The starting date time. Null means undefinitely in the past. </param>
        /// <param name="end">The ending start time. Null means undefinitely in the future. </param>
        /// <returns>True when the date time is between. </returns>
        public static bool IsBetween(this DateTime? dt, DateTime? start, DateTime? end)
        {
            return dt.HasValue && dt.Value.IsBetween(start, end);
        }



        /// <summary>
        /// Convert To Date
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static Date ToDate(this DateTime dt)
        {
            return new Date(dt);
        }

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
        /// 时间戳 Seconds 即自1970年1月1日UTC以来经过的秒数 (等价于ToTimestampSeconds)
        /// </summary>
        public static long Unix(this DateTime dateTime) => (long)(dateTime.ToUniversalTime() - _epoch_utc).TotalSeconds;

        /// <summary>
        /// 时间戳 Seconds 即自1970年1月1日UTC以来经过的秒数 (等价于Unix)
        /// </summary>
        public static long ToTimestampSeconds(this DateTime dateTime) => ToTimestamp(dateTime) / 1000;

        /// <summary>
        /// 时间戳 Milliseconds 即自1970年1月1日UTC以来经过的毫秒数
        /// </summary>
        public static long ToTimestamp(this DateTime dateTime) => (long)Math.Round(((dateTime < _epoch_local ? DateTime.Now : dateTime) - _epoch_local).TotalMilliseconds, MidpointRounding.AwayFromZero);

        /// <summary>
        /// 时间戳 Nanoseconds 即自1970年1月1日UTC以来经过的纳秒数
        /// </summary>
        public static long UnixNano(this DateTime dateTime) => (dateTime.ToUniversalTime() - _epoch_utc).Ticks * 100;

        /// <summary>
        /// 时间范围值
        /// </summary>
        public static string ToDurationString(this TimeSpan duration)
        {
            var a = duration.ToString().Split(':', '.');
            int h = int.Parse(a[0]), m = int.Parse(a[1]), s = int.Parse(a[2]), ms = int.Parse(a[3].Substring(0, 3));
            return $"{(h > 0 ? h + "h" : "")}{(m > 0 ? m + "m" : "")}{(s > 0 ? s + "s" : "")}{(ms > 0 ? ms + "ms" : "")}";
        }

    }
}
