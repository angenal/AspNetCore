using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace WebCore
{
    /// <summary>
    /// Size
    /// </summary>
    public struct Size
    {
        public static readonly Size Zero = new Size(0, SizeUnit.Bytes);

        internal const long OneKB = 1024;
        internal const long OneMB = OneKB * 1024;
        internal const long OneGB = OneMB * 1024;
        internal const long OneTB = OneGB * 1024;

        private readonly SizeUnit _unit;
        private long _valueInBytes;

        /// <summary>
        /// new Size
        /// </summary>
        public Size(int value, SizeUnit unit = SizeUnit.Bytes)
        {
            _unit = unit;
            _valueInBytes = ConvertToBytes(value, unit);
        }
        /// <summary>
        /// new Size
        /// </summary>
        public Size(long value, SizeUnit unit = SizeUnit.Bytes)
        {
            _unit = unit;
            _valueInBytes = ConvertToBytes(value, unit);
        }
        /// <summary>
        /// new Size
        /// </summary>
        public Size(double value, SizeUnit unit = SizeUnit.Bytes)
        {
            _unit = unit;
            _valueInBytes = (long)ConvertToBytes(value, unit);
        }
        /// <summary>
        /// Convert to Size
        /// </summary>
        public static implicit operator Size(int value)
        {
            return new Size(value);
        }
        /// <summary>
        /// Convert to Size
        /// </summary>
        public static implicit operator Size(long value)
        {
            return new Size(value);
        }
        /// <summary>
        /// Convert to Size
        /// </summary>
        public static implicit operator Size(double value)
        {
            return new Size(value);
        }
        /// <summary>
        /// Convert to int
        /// </summary>
        public static implicit operator int(Size value)
        {
            return (int)value._valueInBytes;
        }
        /// <summary>
        /// Convert to long
        /// </summary>
        public static implicit operator long(Size value)
        {
            return value._valueInBytes;
        }
        /// <summary>
        /// Convert to double
        /// </summary>
        public static implicit operator double(Size value)
        {
            return value._valueInBytes;
        }

        private static long ConvertToBytes(long value, SizeUnit unit)
        {
            switch (unit)
            {
                case SizeUnit.Bytes:
                    return value;
                case SizeUnit.KB:
                    return value * OneKB;
                case SizeUnit.MB:
                    return value * OneMB;
                case SizeUnit.GB:
                    return value * OneGB;
                case SizeUnit.TB:
                    return value * OneTB;
                default:
                    throw new NotSupportedException("Not supported size unit: " + unit);
            }
        }

        public static double ConvertToBytes(double value, SizeUnit unit)
        {
            switch (unit)
            {
                case SizeUnit.Bytes:
                    return value;
                case SizeUnit.KB:
                    return value * OneKB;
                case SizeUnit.MB:
                    return value * OneMB;
                case SizeUnit.GB:
                    return value * OneGB;
                case SizeUnit.TB:
                    return value * OneTB;
                default:
                    throw new NotSupportedException("Not supported size unit: " + unit);
            }
        }

        [Pure]
        public long GetValue(SizeUnit requestedUnit)
        {
            switch (requestedUnit)
            {
                case SizeUnit.Bytes:
                    return _valueInBytes;
                case SizeUnit.KB:
                    return _valueInBytes / OneKB;
                case SizeUnit.MB:
                    return _valueInBytes / OneMB;
                case SizeUnit.GB:
                    return _valueInBytes / OneGB;
                case SizeUnit.TB:
                    return _valueInBytes / OneTB;
                default:
                    ThrowUnsupportedSize();
                    return -1;// never hit
            }
        }

        [Pure]
        public double GetDoubleValue(SizeUnit requestedUnit)
        {
            switch (requestedUnit)
            {
                case SizeUnit.Bytes:
                    return _valueInBytes;
                case SizeUnit.KB:
                    return _valueInBytes / (double)OneKB;
                case SizeUnit.MB:
                    return _valueInBytes / (double)OneMB;
                case SizeUnit.GB:
                    return _valueInBytes / (double)OneGB;
                case SizeUnit.TB:
                    return _valueInBytes / (double)OneTB;
                default:
                    ThrowUnsupportedSize();
                    return -1;// never hit
            }
        }

        private void ThrowUnsupportedSize()
        {
            throw new NotSupportedException("Not supported size unit: " + _unit);
        }

        public void Add(int value, SizeUnit unit)
        {
            _valueInBytes += ConvertToBytes(value, unit);
        }

        public void Add(long value, SizeUnit unit)
        {
            _valueInBytes += ConvertToBytes(value, unit);
        }

        public void Set(long newValue, SizeUnit requestedUnit)
        {
            _valueInBytes = ConvertToBytes(newValue, requestedUnit);
        }

        public static bool operator <(Size x, Size y)
        {
            return x._valueInBytes < y._valueInBytes;
        }

        public static bool operator >(Size x, Size y)
        {
            return x._valueInBytes > y._valueInBytes;
        }

        public static bool operator <=(Size x, Size y)
        {
            return x._valueInBytes <= y._valueInBytes;
        }

        public static bool operator >=(Size x, Size y)
        {
            return x._valueInBytes >= y._valueInBytes;
        }

        public static Size operator +(Size x, Size y)
        {
            return new Size(x._valueInBytes + y._valueInBytes, SizeUnit.Bytes);
        }

        public static Size operator -(Size x, Size y)
        {
            return new Size(x._valueInBytes - y._valueInBytes, SizeUnit.Bytes);
        }

        public static Size operator *(Size x, long y)
        {
            return new Size(x._valueInBytes * y, SizeUnit.Bytes);
        }

        public static Size operator *(Size x, double y)
        {
            return new Size((long)(x._valueInBytes * y), SizeUnit.Bytes);
        }

        public static Size operator *(double y, Size x)
        {
            return new Size((long)(x._valueInBytes * y), SizeUnit.Bytes);
        }

        public static Size operator /(Size x, int y)
        {
            return new Size(x._valueInBytes / y, SizeUnit.Bytes);
        }

        public static Size Min(Size x, Size y)
        {
            return x._valueInBytes < y._valueInBytes ? x : y;
        }

        public static Size Max(Size x, Size y)
        {
            return x._valueInBytes > y._valueInBytes ? x : y;
        }

        public static Size Sum(ICollection<Size> sizes)
        {
            return new Size(sizes.Sum(x => x._valueInBytes), SizeUnit.Bytes);
        }

        public override string ToString()
        {
            var v = Math.Abs(_valueInBytes);
            if (v > OneTB)
                return $"{Math.Round(_valueInBytes / (double)OneTB, 4):#,#.####} TB";
            if (v > OneGB)
                return $"{Math.Round(_valueInBytes / (double)OneGB, 3):#,#.###} GB";
            if (v > OneMB)
                return $"{Math.Round(_valueInBytes / (double)OneMB, 2):#,#.##} MB";
            if (v > OneKB)
                return $"{Math.Round(_valueInBytes / (double)OneKB, 2):#,#.##} KB";
            return $"{v:#,#0} Bytes";
        }
    }

    /// <summary>
    /// Size Unit
    /// </summary>
    public enum SizeUnit
    {
        Bytes,
        KB,
        MB,
        GB,
        TB
    }

    public static class Sizes
    {
        public static string Humane(long? size) => size == null ? null : new Size(size.Value).ToString();
    }
}
