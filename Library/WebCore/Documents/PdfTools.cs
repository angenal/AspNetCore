using Spire.Pdf;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace WebCore.Documents
{
    public class PdfTools
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

            if (outputFileFormat == ".html")
            {
                fName = name + ts + outputFileFormat;
                fPath = Path.Combine(dirString, fName);
                if (File.Exists(fPath)) return uriString + "/" + fName;

                var oldFiles = dir.GetFiles(name + "*" + outputFileFormat);
                foreach (var oldFile in oldFiles) oldFile.Delete();

                //var doc = new PdfDocument();
                //if (string.IsNullOrEmpty(password)) doc.LoadFromFile(filename);
                //else doc.LoadFromFile(filename, password);
                //doc.SaveToFile(fPath, FileFormat.HTML);
                //doc.Dispose();

                filename = filename.Replace('\\', '/');
                var src = filename.Substring(filename.IndexOf("wwwroot") + 7);
                var html = PdfTemplate(source.Name, src);
                File.WriteAllText(fPath, html, Encoding.UTF8);

                return uriString + "/" + fName;
            }

            if (outputFileFormat == ".png")
            {
                fName = name + ts + outputFileFormat;
                fPath = Path.Combine(dirString, fName);
                if (File.Exists(fPath)) return uriString + "/" + fName;

                var oldFiles = dir.GetFiles(name + "*" + outputFileFormat);
                foreach (var oldFile in oldFiles) oldFile.Delete();

                var doc = new PdfDocument();
                if (string.IsNullOrEmpty(password)) doc.LoadFromFile(filename);
                else doc.LoadFromFile(filename, password);
                var count = doc.Pages.Count;
                int width = 0, height = 0;
                var images = new Image[count];
                for (var i = 0; i < count; i++)
                {
                    var stream = doc.SaveAsImage(i);
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

        static string PdfTemplate(string title, string url) => $@"<!DOCTYPE html><html>
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
    <title>{title}</title>
    <script src=""../../scripts/jquery/jquery-1.7.1.min.js""></script>
    <script src=""../../scripts/pdfobject/pdfobject.min.js""></script>
</head>
<body>
    <script type=""text/javascript"">PDFObject.embed(""{url}"");</script>
</body>
</html>";

    }
}
