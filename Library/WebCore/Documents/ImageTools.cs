using FluentScheduler;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using WebInterface;
using WebInterface.Settings;

namespace WebCore.Documents
{
    public class ImageTools : IImageTools
    {
        #region 验证码图片

        static readonly int CaptchaCodeLength = 4;
        static readonly ConcurrentDictionary<ulong, string> CaptchaCodeData = new ConcurrentDictionary<ulong, string>();
        //static readonly Tuple<byte[], byte[]> CaptchaCodeArgs = new Tuple<byte[], byte[]>(CryptoFunctions.GenerateNonceBytes(24), CryptoFunctions.GenerateNonceBytes(32));

        /// <summary>
        /// 获取验证码(缓存)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetCaptchaCode(ulong key) => CaptchaCodeData.TryGetValue(key, out string value) ? value : null;

        /// <summary>
        /// 生成验证码(缓存)返回key
        /// </summary>
        /// <param name="time">到期时间</param>
        /// <returns></returns>
        public ulong NewCaptchaCode(DateTime time)
        {
            if (time < DateTime.Now.AddSeconds(5)) return 0;
            var v = new Tuple<string, DateTime>(CaptchaCodeLength.RandomString(), time);
            var k = $"{v.Item1}:{v.Item2.Unix()}:{v.GetHashCode()}".XXH64();
            if (!CaptchaCodeData.TryAdd(k, v.Item1)) return 0;
            JobManager.AddJob(() => CaptchaCodeData.TryRemove(k, out _), s => s.ToRunOnceAt(time));
            return k;
        }

        /// <summary>
        /// 生成验证码图片
        /// </summary>
        /// <param name="captchaCode">验证码参数</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="degree">难度系数 1.低 2.高</param>
        /// <returns></returns>
        public byte[] NewCaptchaCode(string captchaCode, int width = 90, int height = 36, int fontSize = 20, int degree = 1)
        {
            using (var stream = new MemoryStream())
            {
                using (var image = new Bitmap(width, height, PixelFormat.Format64bppArgb))
                {
                    using (var graphics = Graphics.FromImage(image))
                    {
                        //填充背景色
                        graphics.Clear(Color.FromArgb(240, 243, 248));

                        //画图片的背景噪音线
                        var random = new Random(Guid.NewGuid().GetHashCode());
                        if (degree <= 1)
                        {
                            for (int i = 0; i <= 5; i++)
                            {
                                int x1 = random.Next(image.Width), x2 = random.Next(image.Width), y1 = random.Next(image.Height), y2 = random.Next(image.Height);
                                graphics.DrawLine(new Pen(Color.FromArgb(random.Next(255), random.Next(255), random.Next(255))), x1, y1, x2, y2);
                            }
                        }

                        //随机文字颜色
                        int c = random.Next(100) % Constants.TextColors.Length;
                        Color imageTextColor1 = Constants.TextColors[c].Item1, imageTextColor2 = Constants.TextColors[c].Item2;
                        var f = Constants.TextFonts(fontSize);
                        c = random.Next(100) % f.Length;
                        var brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), imageTextColor1, imageTextColor2, 1.2f, true);
                        graphics.DrawString(captchaCode, f[c], brush, 0.2f, 0.2f);

                        //画图片的前景噪音点
                        var r = new Random();
                        for (int i = 0; i < 80; i++)
                        {
                            int x = r.Next(width), y = r.Next(height);
                            image.SetPixel(x, y, Color.FromArgb(r.Next()));
                        }
                        if (degree > 1)
                        {
                            for (var i = 0; i < 25; i++)
                            {
                                int x1 = r.Next(width), x2 = r.Next(width), y1 = r.Next(height), y2 = r.Next(height);
                                graphics.DrawLine(new Pen(Constants.PenColors[r.Next(0, 5)], 1), new PointF(x1, y1), new PointF(x2, y2));
                            }
                            for (var i = 0; i < 25; i++)
                            {
                                int x = r.Next(width), y = r.Next(height);
                                graphics.DrawLine(new Pen(Constants.PenColors[r.Next(0, 5)], 1), new PointF(x, y), new PointF(x + 1, y + 1));
                            }
                        }

                        //画图片的边框线
                        graphics.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);

                        image.Save(stream, ImageFormat.Jpeg);
                    }
                }
                stream.Position = 0;
                return stream.ToArray();
            }
        }

        #endregion

    }
}
