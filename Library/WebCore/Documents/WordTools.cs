using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;
using Spire.Doc.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using WebInterface;

namespace WebCore.Documents
{
    /// <summary>
    /// Spire.Doc - Version 9.4 - Word Tool.
    /// </summary>
    public class WordTools : IWordTools
    {
        /// <summary>
        /// 替换文本时的模板字符串前缀
        /// </summary>
        public static char[] ReplaceCharacterPrefix = "<[{".ToArray();

        /// <summary>
        /// 处理替换文本时的模板字符串
        /// </summary>
        /// <param name="data">数据来源</param>
        /// <param name="prefix">前缀</param>
        /// <param name="suffix">后缀</param>
        public static void ReplaceCharacterPrefixForKey(Dictionary<string, string> data, char prefix = '<', char suffix = '>')
        {
            var keys = data.Keys.ToArray();
            foreach (string key in keys) data.Add(prefix + key + suffix, data[key]);
            foreach (string key in keys) data.Remove(key);
        }

        /// <summary>
        /// 导出 Word 文档(*.doc,*.docx)替换模板文档的书签
        /// </summary>
        /// <param name="templateFile">Word模板文档</param>
        /// <param name="saveFileName">导出文件路径</param>
        /// <param name="dictBookMark">数据字典</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        public static void ExportWithBookMark(string templateFile, string saveFileName, Dictionary<string, string> dictBookMark, string password = null, bool readOnlyProtect = false)
        {
            if (string.IsNullOrEmpty(templateFile))
                throw new ArgumentNullException(nameof(templateFile));
            if (string.IsNullOrEmpty(saveFileName))
                throw new ArgumentNullException(nameof(saveFileName));
            if (dictBookMark == null || dictBookMark.Count == 0)
                throw new ArgumentNullException(nameof(dictBookMark));
            if (!File.Exists(templateFile))
                throw new ArgumentException(templateFile, string.Format("{0} 文件不存在", Path.GetFileName(templateFile)));

            var doc = new Document();
            doc.LoadFromFile(templateFile);
            // 替换文档的书签
            var nav = new BookmarksNavigator(doc);
            foreach (Bookmark bookmark in doc.Bookmarks)
            {
                string key = bookmark.Name;
                if (!dictBookMark.ContainsKey(key)) continue;
                var textRange = FindBookmarkTextRange(bookmark);
                if (textRange == null) continue;
                // 创建文本内容
                var sec = doc.AddSection();
                var range = sec.AddParagraph().AppendText(dictBookMark[key]);
                // 创建文本格式
                ImportContainerMethod.Invoke(range.CharacterFormat, new[] { textRange.CharacterFormat });
                var par1 = sec.Paragraphs[0].Items.FirstItem as ParagraphBase;
                var par2 = sec.Paragraphs[sec.Paragraphs.Count > 1 ? sec.Paragraphs.Count - 1 : 0].Items.LastItem as ParagraphBase;
                var text = new TextBodyPart(new TextBodySelection(par1, par2));
                // 定位书签
                nav.MoveToBookmark(key, true, true);
                // 删除原有的书签内容
                nav.DeleteBookmarkContent(true);
                // 替换为新的书签内容
                nav.ReplaceBookmarkContent(text);
                // 移除内容区域
                doc.Sections.Remove(sec);
            }
            // 替换文档的模板字符串
            foreach (string key in dictBookMark.Keys.Where(k => ReplaceCharacterPrefix.Contains(k[0])))
            {
                foreach (var s in doc.FindAllString(key, true, false))
                {
                    var text = s.GetAsOneRange();
                    text.Text = dictBookMark[key];
                }
            }
            // 加密文档与只读保护
            if (!string.IsNullOrEmpty(password))
            {
                doc.Encrypt(password);
                if (readOnlyProtect) doc.Protect(ProtectionType.AllowOnlyReading, password);
            }
            doc.SaveToFile(saveFileName);
            doc.Dispose();
        }

        /// <summary>
        /// CharacterFormat's ImportContainer protected internal method
        /// </summary>
        static readonly MethodInfo ImportContainerMethod = typeof(Spire.Doc.Formatting.CharacterFormat).GetMethod("ImportContainer", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Find bookmark textRange
        /// </summary>
        /// <param name="bookmark"></param>
        /// <returns></returns>
        static ITextRange FindBookmarkTextRange(Bookmark bookmark)
        {
            var s = bookmark.BookmarkStart.Owner.ChildObjects;
            for (int i = s.Count - 1; i >= 0; i--) if (s[i] is ITextRange tr0) return tr0;
            if (bookmark.BookmarkStart.PreviousSibling is ITextRange tr1) return tr1;
            if (bookmark.BookmarkEnd.NextSibling is ITextRange tr2) return tr2;
            return null;
        }

        /// <summary>
        /// 保护 Word 文档(*.doc,*.docx)
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <param name="password">文件密码</param>
        /// <param name="saveFileName">加密文件路径</param>
        /// <param name="encryptOpen">输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        public void EncryptProtect(string filename, string password, string saveFileName = null, bool encryptOpen = true, bool readOnlyProtect = false)
        {
            if (string.IsNullOrEmpty(password) || (!encryptOpen && !readOnlyProtect)) return;
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename))
                throw new ArgumentException(filename, string.Format("{0} 文件不存在", Path.GetFileName(filename)));

            var doc = new Document();
            doc.LoadFromFile(filename);
            if (encryptOpen) doc.Encrypt(password);
            if (readOnlyProtect) doc.Protect(ProtectionType.AllowOnlyReading, password);
            if (!string.IsNullOrEmpty(saveFileName)) filename = saveFileName;
            doc.SaveToFile(filename);
            doc.Dispose();
        }

        /// <summary>
        /// 导出图片png格式
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <param name="outputDirectory">保存图片的相对路径目录</param>
        /// <param name="httpRootPath">http绝对路径</param>
        /// <param name="password">文件打开密码</param>
        /// <returns></returns>
        public string ImagePreview(string filename, string outputDirectory, string httpRootPath = "", string password = "")
        {
            return SaveToFile(filename, outputDirectory, ".png", httpRootPath, password);
        }

        /// <summary>
        /// 导出文档pdf格式
        /// </summary>
        /// <param name="filename">word文件路径</param>
        /// <param name="outputDirectory">保存的相对路径目录</param>
        /// <param name="httpRootPath">http绝对路径</param>
        /// <param name="password">文件打开密码</param>
        /// <returns></returns>
        public string PdfPreview(string filename, string outputDirectory, string httpRootPath = "", string password = "")
        {
            return SaveToFile(filename, outputDirectory, ".pdf", httpRootPath, password);
        }

        /// <summary>
        /// 导出网页html格式
        /// </summary>
        /// <param name="filename">word文件路径</param>
        /// <param name="outputDirectory">保存的相对路径目录</param>
        /// <param name="httpRootPath">http绝对路径</param>
        /// <param name="password">文件打开密码</param>
        /// <returns></returns>
        public string HtmlPreview(string filename, string outputDirectory, string httpRootPath = "", string password = "")
        {
            return SaveToFile(filename, outputDirectory, ".html", httpRootPath, password);
        }

        string SaveToFile(string filename, string outputDirectory, string outputFileFormat, string httpRootPath, string password)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException(nameof(filename));
            if (string.IsNullOrEmpty(outputDirectory))
                throw new ArgumentNullException(nameof(outputDirectory));
            var source = new FileInfo(filename);
            if (!source.Exists)
                throw new ArgumentException(filename, string.Format("{0} 文件不存在", source.Name));
            if (Path.IsPathRooted(outputDirectory))
                throw new ArgumentException(outputDirectory, string.Format("{0} 相对路径错误", nameof(outputDirectory)));
            if (string.IsNullOrEmpty(httpRootPath))
                throw new ArgumentNullException(nameof(httpRootPath));
            var uriString = httpRootPath + outputDirectory.Trim('/');
            if (!Uri.TryCreate(uriString, UriKind.Absolute, out _))
                throw new ArgumentException(outputDirectory, string.Format("{0} 相对路径错误", nameof(outputDirectory)));
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            return SaveToFile(source, outputDirectory, outputFileFormat, uriString, password);
        }

        /// <summary>
        /// 另存 Word 文档(*.pdf,*.html,*.png)
        /// </summary>
        /// <param name="source">来源文件(*.doc,*.docx)</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="outputFileFormat">选择一种输出格式(pdf,html,png)</param>
        /// <param name="uriString">返回文件路径前缀网址</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="name">返回文件名(不包含文件扩展类型)</param>
        /// <param name="ts">返回文件名后缀</param>
        /// <returns></returns>
        public string SaveToFile(FileInfo source, string outputDirectory, string outputFileFormat, string uriString = null, string password = null, string name = null, string ts = null)
        {
            string filename = source.FullName, dirString = outputDirectory, fName, fPath;
            if (name == null) name = Path.GetFileNameWithoutExtension(filename);
            if (ts == null) ts = source.LastWriteTimeHex();
            var dir = new DirectoryInfo(dirString);

            if (outputFileFormat == "pdf" || outputFileFormat == ".pdf")
            {
                fName = name + ts + outputFileFormat;
                fPath = Path.Combine(dirString, fName);
                if (File.Exists(fPath)) return uriString + "/" + fName;

                var oldFiles = dir.GetFiles(name + "*" + outputFileFormat);
                foreach (var oldFile in oldFiles) oldFile.Delete();

                var doc = new Document();
                if (string.IsNullOrEmpty(password)) doc.LoadFromFile(filename);
                else doc.LoadFromFile(filename, FileFormat.Auto, password);
                doc.SaveToFile(fPath, FileFormat.PDF);
                doc.Dispose();

                return uriString + "/" + fName;
            }

            if (outputFileFormat == "html" || outputFileFormat == ".html")
            {
                fName = name + ts + outputFileFormat;
                fPath = Path.Combine(dirString, fName);
                if (File.Exists(fPath)) return uriString + "/" + fName;

                var oldFiles = dir.GetFiles(name + "*" + outputFileFormat);
                foreach (var oldFile in oldFiles)
                {
                    var tempImages = Path.Combine(dirString, Path.GetFileNameWithoutExtension(oldFile.FullName) + "_images");
                    if (Directory.Exists(tempImages)) Directory.Delete(tempImages, true);
                    oldFile.Delete();
                }

                var doc = new Document();
                if (string.IsNullOrEmpty(password)) doc.LoadFromFile(filename);
                else doc.LoadFromFile(filename, FileFormat.Auto, password);
                doc.SaveToFile(fPath, FileFormat.Html);
                doc.Dispose();

                // merge html css
                var fileEncoding = Encoding.UTF8;
                var htmlContent = File.ReadAllText(fPath, fileEncoding);
                string cssExtension = "_styles.css", cssFile = Path.Combine(dirString, name + ts + cssExtension);
                var cssContent = File.ReadAllText(cssFile, fileEncoding);
                var content = htmlContent.Replace($"<link href=\"{name}{ts}{cssExtension}\" type=\"text/css\" rel=\"stylesheet\"/>", "<style>" + cssContent + "</style>");
                File.WriteAllText(fPath, content, fileEncoding);
                File.Delete(cssFile);

                return uriString + "/" + fName;
            }

            if (outputFileFormat == "png" || outputFileFormat == ".png")
            {
                fName = name + ts + outputFileFormat;
                fPath = Path.Combine(dirString, fName);
                if (File.Exists(fPath)) return uriString + "/" + fName;

                var oldFiles = dir.GetFiles(name + "*" + outputFileFormat);
                foreach (var oldFile in oldFiles) oldFile.Delete();

                var doc = new Document();
                if (string.IsNullOrEmpty(password)) doc.LoadFromFile(filename);
                else doc.LoadFromFile(filename, FileFormat.Auto, password);
                var count = doc.PageCount;
                int width = 0, height = 0;
                var images = new SkiaSharp.SKImage[count];
                for (var i = 0; i < count; i++)
                {
                    var stream = doc.SaveToImages(i, ImageType.Metafile);
                    if (stream == null) continue;
                    images[i] = stream;
                    width = images[i].Width;
                    height += images[i].Height;
                }
                doc.Dispose();

                // merge image
                var b = new SkiaSharp.SKBitmap(width, height);
                using (var g = new SkiaSharp.SKCanvas(b))
                {
                    g.Clear();
                    for (int i = 0, y = 0; i < count; i++)
                    {
                        g.DrawImage(images[i], new SkiaSharp.SKPoint(0, y));
                        y += images[i].Height;
                    }
                }
                using (var m = new MemoryStream())
                {
                    using (var w = new SkiaSharp.SKManagedWStream(m)) b.Encode(w, SkiaSharp.SKEncodedImageFormat.Png, 50);
                    File.WriteAllBytes(fPath, m.ToArray());
                }
                foreach (var img in images) img.Dispose();
                b.Dispose();

                return uriString + "/" + fName;
            }

            throw new ArgumentNullException(nameof(outputFileFormat));
        }
    }
}
