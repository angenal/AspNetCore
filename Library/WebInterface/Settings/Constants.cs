using System;
using System.Drawing;

namespace WebInterface.Settings
{
    /// <summary>
    /// 常量数据
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// 文件大小单位
        /// </summary>
        public static class Size
        {
            public const int KB = 1024;
            public const int MB = 1024 * KB;
            public const int GB = 1024 * MB;
            public const long TB = 1024 * (long)GB;
            public const long PB = 1024 * (long)TB;
        }

        /// <summary>
        /// 随机文字颜色
        /// </summary>
        public static readonly Tuple<Color, Color>[] TextColors =
        {
            new Tuple<Color, Color>(Color.FromArgb(65, 133, 235), Color.FromArgb(142, 24, 232)),
            new Tuple<Color, Color>(Color.FromArgb(52, 116, 235), Color.FromArgb(251, 40, 40)),
            new Tuple<Color, Color>(Color.FromArgb(200, 68, 235), Color.FromArgb(61, 53, 235)),
            new Tuple<Color, Color>(Color.FromArgb(255, 95, 89), Color.FromArgb(95, 13, 255))
        };

        /// <summary>
        /// 随机笔刷颜色
        /// </summary>
        public static readonly Color[] PenColors =
        {
            Color.FromArgb(37, 72, 91),
            Color.FromArgb(68, 24, 25),
            Color.FromArgb(17, 46, 2),
            Color.FromArgb(70, 16, 100),
            Color.FromArgb(24, 88, 74)
        };

        /// <summary>
        /// 随机文字字体
        /// </summary>
        public static Font[] TextFonts(int fontSize) => new Font[]
        {
            new Font(new FontFamily("Arial"), fontSize, FontStyle.Bold | FontStyle.Italic),
            new Font(new FontFamily("Georgia"), fontSize, FontStyle.Bold | FontStyle.Italic),
            new Font(new FontFamily("Times New Roman"), fontSize, FontStyle.Bold | FontStyle.Italic),
            new Font(new FontFamily("Comic Sans MS"), fontSize, FontStyle.Bold | FontStyle.Italic)
        };
    }
}
