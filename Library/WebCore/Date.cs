using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace WebCore
{
    /// <summary>
    /// Date 专门用来处理日期的类，不会有‘时分秒’的干扰
    /// </summary>
    public struct Date : IComparable, IFormattable, ISerializable, IComparable<Date>, IEquatable<Date>
    {
        /// <summary>
        /// 这个变量的时分秒始终是0
        /// </summary>
        private readonly DateTime _dt;
        /// <summary>
        /// DateTime.MaxValue
        /// </summary>
        public static readonly Date MaxValue = new Date(DateTime.MaxValue);
        /// <summary>
        /// DateTime.MinValue
        /// </summary>
        public static readonly Date MinValue = new Date(DateTime.MinValue);

        /// <summary>
        /// new Date
        /// </summary>
        public Date(int year, int month, int day)
        {
            _dt = new DateTime(year, month, day);
        }
        /// <summary>
        /// new Date
        /// </summary>
        public Date(DateTime dateTime)
        {
            _dt = dateTime.AddTicks(-dateTime.Ticks % TimeSpan.TicksPerDay);
        }

        /// <summary>
        /// Startup DateTime
        /// </summary>
        public static DateTime Startup = Now();

        /// <summary>
        /// 当前本地时间 DateTime.Now
        /// </summary>
        /// <param name="timeZone">时区:上海"Asia/Shanghai"</param>
        /// <returns></returns>
        public static DateTime Now(string timeZone = "Asia/Shanghai")
        {
            return NodaTime.SystemClock.Instance.GetCurrentInstant().InZone(NodaTime.DateTimeZoneProviders.Tzdb[timeZone]).ToDateTimeUnspecified();
        }

        /// <summary>
        /// 当前标准时间 DateTime.UtcNow
        /// </summary>
        /// <returns></returns>
        public static DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        ///
        /// </summary>
        public static TimeSpan operator -(Date d1, Date d2)
        {
            return d1._dt - d2._dt;
        }

        /// <summary>
        ///
        /// </summary>
        public static Date operator -(Date d, TimeSpan t)
        {
            return new Date(d._dt - t);
        }

        /// <summary>
        ///
        /// </summary>
        public static bool operator !=(Date d1, Date d2)
        {
            return d1._dt != d2._dt;
        }

        /// <summary>
        ///
        /// </summary>
        public static Date operator +(Date d, TimeSpan t)
        {
            return new Date(d._dt + t);
        }

        /// <summary>
        ///
        /// </summary>
        public static bool operator <(Date d1, Date d2)
        {
            return d1._dt < d2._dt;
        }

        /// <summary>
        ///
        /// </summary>
        public static bool operator <=(Date d1, Date d2)
        {
            return d1._dt <= d2._dt;
        }

        /// <summary>
        ///
        /// </summary>
        public static bool operator ==(Date d1, Date d2)
        {
            return d1._dt == d2._dt;
        }

        /// <summary>
        ///
        /// </summary>
        public static bool operator >(Date d1, Date d2)
        {
            return d1._dt > d2._dt;
        }

        /// <summary>
        ///
        /// </summary>
        public static bool operator >=(Date d1, Date d2)
        {
            return d1._dt >= d2._dt;
        }

        /// <summary>
        ///
        /// </summary>
        public static implicit operator DateTime(Date d)
        {
            return d._dt;
        }

        /// <summary>
        ///
        /// </summary>
        public static explicit operator Date(DateTime d)
        {
            return new Date(d);
        }

        /// <summary>
        /// 月第几天
        /// </summary>
        public int Day
        {
            get
            {
                return _dt.Day;
            }
        }

        /// <summary>
        /// 周第几天
        /// </summary>
        public DayOfWeek DayOfWeek
        {
            get
            {
                return _dt.DayOfWeek;
            }
        }

        /// <summary>
        /// 年第几天
        /// </summary>
        public int DayOfYear
        {
            get
            {
                return _dt.DayOfYear;
            }
        }

        /// <summary>
        /// 月
        /// </summary>
        public int Month
        {
            get
            {
                return _dt.Month;
            }
        }

        /// <summary>
        /// 今日
        /// </summary>
        public static Date Today
        {
            get
            {
                return new Date(DateTime.Today);
            }
        }

        /// <summary>
        /// 年
        /// </summary>
        public int Year
        {
            get
            {
                return _dt.Year;
            }
        }

        /// <summary>
        /// 返回上一天
        /// </summary>
        public Date Yesterday
        {
            get
            {
                return AddDays(-1);
            }
        }

        /// <summary>
        /// 返回下一天
        /// </summary>
        public Date Tomorrow
        {
            get
            {
                return AddDays(1);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public Date AddDays(int value)
        {
            return new Date(_dt.AddDays(value));
        }

        /// <summary>
        ///
        /// </summary>
        public Date AddMonths(int value)
        {
            return new Date(_dt.AddMonths(value));
        }

        /// <summary>
        ///
        /// </summary>
        public Date AddYears(int value)
        {
            return new Date(_dt.AddYears(value));
        }

        /// <summary>
        ///
        /// </summary>
        public static int Compare(Date d1, Date d2)
        {
            return d1.CompareTo(d2);
        }

        /// <summary>
        ///
        /// </summary>
        public int CompareTo(Date value)
        {
            return _dt.CompareTo(value._dt);
        }

        /// <summary>
        ///
        /// </summary>
        public int CompareTo(object value)
        {
            return _dt.CompareTo(value);
        }

        /// <summary>
        ///
        /// </summary>
        public static int DaysInMonth(int year, int month)
        {
            return DateTime.DaysInMonth(year, month);
        }

        /// <summary>
        ///
        /// </summary>
        public bool Equals(Date value)
        {
            return _dt.Equals(value._dt);
        }

        /// <summary>
        ///
        /// </summary>
        public override bool Equals(object value)
        {
            return value is Date && _dt.Equals(((Date)value)._dt);
        }

        /// <summary>
        ///
        /// </summary>
        public override int GetHashCode()
        {
            return _dt.GetHashCode();
        }

        /// <summary>
        ///
        /// </summary>
        public static bool Equals(Date d1, Date d2)
        {
            return d1._dt.Equals(d2._dt);
        }

        /// <summary>
        ///
        /// </summary>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ticks", _dt.Ticks);
        }

        /// <summary>
        ///
        /// </summary>
        public static bool IsLeapYear(int year)
        {
            return DateTime.IsLeapYear(year);
        }

        /// <summary>
        ///
        /// </summary>
        public static Date Parse(string s)
        {
            return new Date(DateTime.Parse(s));
        }

        /// <summary>
        ///
        /// </summary>
        public static Date Parse(string s, IFormatProvider provider)
        {
            return new Date(DateTime.Parse(s, provider));
        }

        /// <summary>
        ///
        /// </summary>
        public static Date Parse(string s, IFormatProvider provider, DateTimeStyles style)
        {
            return new Date(DateTime.Parse(s, provider, style));
        }

        /// <summary>
        ///
        /// </summary>
        public static Date ParseExact(string s, string format, IFormatProvider provider)
        {
            return new Date(DateTime.ParseExact(s, format, provider));
        }

        /// <summary>
        ///
        /// </summary>
        public static Date ParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style)
        {
            return new Date(DateTime.ParseExact(s, format, provider, style));
        }

        /// <summary>
        ///
        /// </summary>
        public static Date ParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style)
        {
            return new Date(DateTime.ParseExact(s, formats, provider, style));
        }

        /// <summary>
        /// 相减时间段
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TimeSpan Subtract(Date value)
        {
            return this - value;
        }

        /// <summary>
        /// 减去时间段
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Date Subtract(TimeSpan value)
        {
            return this - value;
        }

        /// <summary>
        /// 获取形如"Thursday, July 4, 2013"
        /// </summary>
        /// <returns></returns>
        public string ToLongString()
        {
            return _dt.ToLongDateString();
        }

        /// <summary>
        /// 获取形如"7/4/2013"
        /// </summary>
        /// <returns></returns>
        public string ToShortString()
        {
            return _dt.ToShortDateString();
        }

        /// <summary>
        /// 获取形如"2013-07-04"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _dt.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 格式自定义
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public string ToString(IFormatProvider provider)
        {
            return _dt.ToString(provider);
        }

        /// <summary>
        /// 格式形如yyyy-MM-dd
        /// </summary>
        /// <param name="format">格式"s"返回"2013-07-04"</param>
        /// <returns></returns>
        public string ToString(string format)
        {
            return format == "s" || format == "O" || format == "o" ? ToString("yyyy-MM-dd") : _dt.ToString(format);
        }

        /// <summary>
        /// 格式自定义
        /// </summary>
        public string ToString(string format, IFormatProvider provider)
        {
            return _dt.ToString(format, provider);
        }

        /// <summary>
        /// 获取形如20150702这样格式的日期
        /// </summary>
        public int ToInt()
        {
            return _dt.Year * 10000 + _dt.Month * 100 + _dt.Day;
        }

        /// <summary>
        /// 获取形如20150702这样格式的日期
        /// </summary>
        public string ToIntString()
        {
            return ToInt().ToString();
        }

        /// <summary>
        /// DateTime.TryParse
        /// </summary>
        public static bool TryParse(string s, out Date result)
        {
            bool success = DateTime.TryParse(s, out DateTime d);
            result = new Date(d);
            return success;
        }

        /// <summary>
        /// DateTime.TryParse
        /// </summary>
        public static bool TryParse(string s, IFormatProvider provider, DateTimeStyles style, out Date result)
        {
            bool success = DateTime.TryParse(s, provider, style, out DateTime d);
            result = new Date(d);
            return success;
        }

        /// <summary>
        /// DateTime.TryParseExact
        /// </summary>
        public static bool TryParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style, out Date result)
        {
            bool success = DateTime.TryParseExact(s, format, provider, style, out DateTime d);
            result = new Date(d);
            return success;
        }

        /// <summary>
        /// DateTime.TryParseExact
        /// </summary>
        public static bool TryParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style, out Date result)
        {
            bool success = DateTime.TryParseExact(s, formats, provider, style, out DateTime d);
            result = new Date(d);
            return success;
        }
    }
}
