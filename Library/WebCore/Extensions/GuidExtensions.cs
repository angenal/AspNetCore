using System;

namespace WebCore
{
    public static class GuidExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static int ToInt(this Guid guid, byte shift = 8)
        {
            var res = 0;
            var bytes = guid.ToByteArray();

            for (var i = 0; i < shift; i++)
                res += bytes[i] << i * 8;

            return Math.Abs(res);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static string ToIntx8(this Guid guid, byte shift = 8)
        {
            var c = ToInt(guid, shift);
            return c == 0 ? string.Empty : c.ToString("x8");
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static string ToIntX8(this Guid guid, byte shift = 8)
        {
            var c = ToInt(guid, shift);
            return c == 0 ? string.Empty : c.ToString("X8");
        }
    }
}
