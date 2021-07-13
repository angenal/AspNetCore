using Spire.Presentation;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WebCore.Documents
{
    public class PptTools
    {
        /// <summary>
        /// 导出文档
        /// </summary>
        /// <param name="source"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="outputFileFormat"></param>
        /// <param name="uriString"></param>
        /// <param name="password"></param>
        /// <param name="name"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string SaveToFile(FileInfo source, string outputDirectory, string outputFileFormat, string uriString = null, string password = null, string name = null, string ts = null)
        {
            string filename = source.FullName, dirString = outputDirectory, fName, fPath;
            if (name == null) name = Path.GetFileNameWithoutExtension(filename);
            if (ts == null) ts = source.LastWriteTimeHex();
            var dir = new DirectoryInfo(dirString);

            if (outputFileFormat == ".pdf")
            {
                fName = name + ts + outputFileFormat;
                fPath = Path.Combine(dirString, fName);
                if (File.Exists(fPath)) return uriString + "/" + fName;

                var oldFiles = dir.GetFiles(name + "*" + outputFileFormat);
                foreach (var oldFile in oldFiles) oldFile.Delete();

                var doc = new Presentation();
                if (string.IsNullOrEmpty(password)) doc.LoadFromFile(filename);
                else doc.LoadFromFile(filename, FileFormat.Auto, password);
                doc.SaveToFile(fPath, FileFormat.PDF);
                doc.Dispose();

                return uriString + "/" + fName;
            }

            if (outputFileFormat == ".html")
            {
                fName = name + ts + outputFileFormat;
                fPath = Path.Combine(dirString, fName);
                if (File.Exists(fPath)) return uriString + "/" + fName;

                var doc = new Presentation();
                if (string.IsNullOrEmpty(password)) doc.LoadFromFile(filename);
                else doc.LoadFromFile(filename, FileFormat.Auto, password);
                doc.SaveToFile(fPath, FileFormat.Html);
                doc.Dispose();

                return uriString + "/" + fName;
            }

            if (outputFileFormat == ".png")
            {
                fName = name + ts + outputFileFormat;
                fPath = Path.Combine(dirString, fName);
                if (File.Exists(fPath)) return uriString + "/" + fName;

                var oldFiles = dir.GetFiles(name + "*" + outputFileFormat);
                foreach (var oldFile in oldFiles) oldFile.Delete();

                var doc = new Presentation();
                if (string.IsNullOrEmpty(password)) doc.LoadFromFile(filename);
                else doc.LoadFromFile(filename, FileFormat.Auto, password);
                var count = doc.Slides.Count;
                int width = 0, height = 0;
                var images = new Image[count];
                for (var i = 0; i < count; i++)
                {
                    var stream = doc.Slides[i].SaveAsImage();
                    if (stream == null) continue;
                    images[i] = Image.FromStream(stream);
                    width = images[i].Width;
                    height += images[i].Height;
                }
                doc.Dispose();

                // merge image
                var b = new Bitmap(width, height);
                var g = Graphics.FromImage(b);
                for (int i = 0, y = 0; i < count; i++)
                {
                    if (images[i] == null) continue;
                    g.DrawImage(images[i], new Point(0, y));
                    y += images[i].Height;
                }
                b.Save(fPath, ImageFormat.Png);
                foreach (var img in images) img.Dispose();
                b.Dispose(); g.Dispose();

                return uriString + "/" + fName;
            }

            throw new ArgumentNullException(nameof(outputFileFormat));
        }
    }
}
