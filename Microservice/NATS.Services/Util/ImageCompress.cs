using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace NATS.Services.Util
{
    /// <summary>
    /// 图片压缩
    /// </summary>
    public class ImageCompress
    {
        /// <summary>
        /// 支持的图片文件格式
        /// </summary>
        static readonly string[] _supported = { ".png", ".jpg", ".jpeg", ".gif" };
        /// <summary>
        /// 压缩程序所在目录
        /// </summary>
        static readonly string _cwd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools");

        /// <summary>
        /// 压缩图片文件
        /// </summary>
        /// <param name="fileName">来源图片文件地址</param>
        /// <param name="cmdArguments">命令行参数</param>
        /// <param name="lossy">有损压缩模式</param>
        /// <param name="overwrite">覆盖文件</param>
        /// <returns></returns>
        public static ImageCompressionResult CompressFile(string fileName, string cmdArguments = null, bool lossy = true, bool overwrite = true)
        {
            if (!Uri.IsWellFormedUriString(fileName, UriKind.RelativeOrAbsolute) && !File.Exists(fileName))
                return null;

            string sourceFile = fileName, extension = Path.GetExtension(sourceFile).ToLowerInvariant();
            string tempFile = sourceFile.Replace(extension, Path.GetRandomFileName().Split('.')[0] + extension);
            string targetFile = Path.ChangeExtension(tempFile, extension);
            string arguments = GetArguments(sourceFile, cmdArguments, targetFile, extension, lossy);
            if (arguments == null) return null;

            var start = new ProcessStartInfo("cmd")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = _cwd,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var stopwatch = Stopwatch.StartNew();
            using (var process = Process.Start(start)) process.WaitForExit();
            stopwatch.Stop();

            var result = new ImageCompressionResult(sourceFile, targetFile, stopwatch.Elapsed);
            if (overwrite) HandleResult(result);
            File.Delete(tempFile);
            return result;
        }

        static string GetArguments(string sourceFile, string cmdArguments, string targetFile, string extension, bool lossy)
        {
            switch (extension)
            {
                case ".png":
                    File.Copy(sourceFile, targetFile, true);

                    if (lossy)
                        return string.Format(CultureInfo.CurrentCulture, "/c {1} \"{0}\"", targetFile,
                            string.IsNullOrEmpty(cmdArguments) ? "pingo -s9 -pngpalette=40 -strip -q" : cmdArguments);
                    else
                        return string.Format(CultureInfo.CurrentCulture, "/c {1} \"{0}\"", targetFile,
                            string.IsNullOrEmpty(cmdArguments) ? "pingo -auto=100 -s9 -pngpalette=80 -strip -q" : cmdArguments);

                case ".jpg":
                case ".jpeg":
                    if (lossy)
                        return string.Format(CultureInfo.CurrentCulture, "/c {2} -outfile \"{1}\" \"{0}\"", sourceFile, targetFile,
                            string.IsNullOrEmpty(cmdArguments) ? "cjpeg -quality 60,40 -optimize -dct float -smooth 5" : cmdArguments);
                    else
                        return string.Format(CultureInfo.CurrentCulture, "/c {2} -outfile \"{1}\" \"{0}\"", sourceFile, targetFile,
                            string.IsNullOrEmpty(cmdArguments) ? "jpegtran -copy none -optimize -progressive" : cmdArguments);

                case ".gif":
                    return string.Format(CultureInfo.CurrentCulture, "/c {2} \"{0}\" --output=\"{1}\"", sourceFile, targetFile,
                        string.IsNullOrEmpty(cmdArguments) ? "gifsicle -O3" : cmdArguments);
            }

            return null;
        }

        public static bool IsFileSupported(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            return _supported.Any(s => s.Equals(ext, StringComparison.OrdinalIgnoreCase));
        }

        static void HandleResult(ImageCompressionResult result)
        {
            if (result.Saving > 0 && result.ResultFileSize > 0 && File.Exists(result.ResultFileName))
            {
                File.Copy(result.ResultFileName, result.OriginalFileName, true);
                File.Delete(result.ResultFileName);
            }
        }
    }

    public class ImageCompressionResult
    {
        public ImageCompressionResult(string originalFileName, string resultFileName, TimeSpan elapsed)
        {
            FileInfo original = new FileInfo(originalFileName), result = new FileInfo(resultFileName);

            if (original.Exists)
            {
                OriginalFileName = original.FullName;
                OriginalFileSize = original.Length;
            }

            if (result.Exists)
            {
                ResultFileName = result.FullName;
                ResultFileSize = result.Length;
            }

            Elapsed = elapsed;
            Processed = true;
        }

        public long OriginalFileSize { get; set; }
        public string OriginalFileName { get; set; }
        public long ResultFileSize { get; set; }
        public string ResultFileName { get; set; }
        public bool Processed { get; set; }
        public TimeSpan Elapsed { get; set; }

        public long Saving
        {
            get { return Math.Max(OriginalFileSize - ResultFileSize, 0); }
        }

        public double Percent
        {
            get
            {
                return Math.Round(100 - (double)ResultFileSize / OriginalFileSize * 100, 1);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Optimized " + Path.GetFileName(OriginalFileName) + " in " + Math.Round(Elapsed.TotalMilliseconds / 1000, 2) + " seconds");
            sb.AppendLine("Before: " + OriginalFileSize + " bytes");
            sb.AppendLine("After: " + ResultFileSize + " bytes");
            sb.AppendLine("Saving: " + Saving + " bytes / " + Percent + "%");

            return sb.ToString();
        }
    }
}
