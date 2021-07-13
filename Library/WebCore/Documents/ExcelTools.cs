using Spire.Xls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace WebCore.Documents
{
    public class ExcelTools : IExcelTools
    {
        public void ExportWithList(string templateFile, string saveFileName, IEnumerable<Hashtable> list, Dictionary<string, string> columnHeaders, bool copyStyles = true, string password = null, bool readOnlyProtect = false)
        {
            if (string.IsNullOrEmpty(templateFile))
                throw new ArgumentNullException(nameof(templateFile));
            if (string.IsNullOrEmpty(saveFileName))
                throw new ArgumentNullException(nameof(saveFileName));
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (!File.Exists(templateFile))
                throw new ArgumentException(templateFile, string.Format("{0} 文件不存在", Path.GetFileName(templateFile)));

            var doc = new Workbook();
            doc.LoadFromFile(templateFile);
            if (doc.Worksheets.Count == 0)
            {
                doc.Dispose();
                return;
            }
            // 逐行导入数据
            var sheet = doc.Worksheets[0];
            if (list.Any())
            {
                var keys = list.First().Keys.OfType<string>();
                var columns = columnHeaders.Keys.Where(key => keys.Contains(columnHeaders[key])).ToArray();
                var headers = columns.Select(column => column.Substring(0, column.Length - 1)).ToArray();
                int rowIndex = sheet.FirstRow, templateRow = rowIndex + 1, columnsLength = columns.Length;
                foreach (Hashtable item in list)
                {
                    rowIndex++;
                    if (copyStyles && rowIndex > templateRow)
                    {
                        sheet.InsertRow(rowIndex);
                        sheet.CopyRow(sheet.Rows[1], sheet, rowIndex, CopyRangeOptions.All);
                    }
                    for (int i = 0; i < columnsLength; i++)
                    {
                        sheet[headers[i] + rowIndex].Text = item[columnHeaders[columns[i]]]?.ToString();
                    }
                }
            }
            // 加密文档与只读保护
            if (!string.IsNullOrEmpty(password))
            {
                if (readOnlyProtect) doc.Protect(password, true, true);
                else doc.Protect(password);
            }
            doc.SaveToFile(saveFileName);
            doc.Dispose();
        }

        public void ExportWithList(string templateFile, Stream outputStream, IEnumerable<Hashtable> list, Dictionary<string, string> columnHeaders, bool copyStyles = true, string password = null, bool readOnlyProtect = false)
        {
            if (string.IsNullOrEmpty(templateFile))
                throw new ArgumentNullException(nameof(templateFile));
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (!File.Exists(templateFile))
                throw new ArgumentException(templateFile, string.Format("{0} 文件不存在", Path.GetFileName(templateFile)));

            var doc = new Workbook();
            doc.LoadFromFile(templateFile);
            if (doc.Worksheets.Count == 0)
            {
                doc.Dispose();
                return;
            }
            // 逐行导入数据
            var sheet = doc.Worksheets[0];
            if (list.Any())
            {
                var keys = list.First().Keys.OfType<string>();
                var columns = columnHeaders.Keys.Where(key => keys.Contains(columnHeaders[key])).ToArray();
                var headers = columns.Select(column => column.Substring(0, column.Length - 1)).ToArray();
                int rowIndex = sheet.FirstRow, templateRow = rowIndex + 1, columnsLength = columns.Length;
                foreach (Hashtable item in list)
                {
                    rowIndex++;
                    if (copyStyles && rowIndex > templateRow)
                    {
                        sheet.InsertRow(rowIndex);
                        sheet.CopyRow(sheet.Rows[1], sheet, rowIndex, CopyRangeOptions.All);
                    }
                    for (int i = 0; i < columnsLength; i++)
                    {
                        sheet[headers[i] + rowIndex].Text = item[columnHeaders[columns[i]]]?.ToString();
                    }
                }
            }
            // 加密文档与只读保护
            if (!string.IsNullOrEmpty(password))
            {
                if (readOnlyProtect) doc.Protect(password, true, true);
                else doc.Protect(password);
            }
            doc.SaveToStream(outputStream);
            doc.Dispose();
        }

        public void ExportWithDataTable(string saveFileName, DataTable dataTable, bool columnHeaders = true, int firstRow = 1, int firstColumn = 1, Action<Worksheet> action = null, string password = null, bool readOnlyProtect = false)
        {
            if (string.IsNullOrEmpty(saveFileName))
                throw new ArgumentNullException(nameof(saveFileName));
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            var doc = new Workbook();
            doc.Version = ExcelVersion.Version2007; // 指定版本07及以上版本最多可以插入1048576行数据
            if (dataTable.Rows.Count > 1048575) doc.Version = ExcelVersion.Version2013;
            doc.Worksheets.Clear();
            if (dataTable.Rows.Count > 0)
            {
                var sheet = doc.CreateEmptySheet(string.IsNullOrEmpty(dataTable.TableName) ? "Sheet1" : dataTable.TableName);
                sheet.InsertDataTable(dataTable, columnHeaders, firstRow, firstColumn);
                action?.Invoke(sheet);
                // 加密文档与只读保护
                if (!string.IsNullOrEmpty(password))
                {
                    if (readOnlyProtect) doc.Protect(password, true, true);
                    else doc.Protect(password);
                }
            }
            doc.SaveToFile(saveFileName, doc.Version);
            doc.Dispose();
        }

        public void ExportWithDataTable(Stream outputStream, DataTable dataTable, bool columnHeaders = true, int firstRow = 1, int firstColumn = 1, Action<Worksheet> action = null, string password = null, bool readOnlyProtect = false)
        {
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            var doc = new Workbook();
            doc.Version = ExcelVersion.Version2007; // 指定版本07及以上版本最多可以插入1048576行数据
            if (dataTable.Rows.Count > 1048575) doc.Version = ExcelVersion.Version2013;
            doc.Worksheets.Clear();
            if (dataTable.Rows.Count > 0)
            {
                var sheet = doc.CreateEmptySheet(string.IsNullOrEmpty(dataTable.TableName) ? "Sheet1" : dataTable.TableName);
                sheet.InsertDataTable(dataTable, columnHeaders, firstRow, firstColumn);
                action?.Invoke(sheet);
                // 加密文档与只读保护
                if (!string.IsNullOrEmpty(password))
                {
                    if (readOnlyProtect) doc.Protect(password, true, true);
                    else doc.Protect(password);
                }
            }
            doc.SaveToStream(outputStream);
            doc.Dispose();
        }

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

                var doc = new Workbook();
                if (!string.IsNullOrEmpty(password)) doc.OpenPassword = password;
                doc.LoadFromFile(filename);
                doc.SaveToFile(fPath, FileFormat.PDF);
                doc.Dispose();

                return uriString + "/" + fName;
            }

            if (outputFileFormat == ".html")
            {
                fName = name + ts + outputFileFormat;
                fPath = Path.Combine(dirString, fName);
                if (File.Exists(fPath)) return uriString + "/" + fName;

                var oldFiles = dir.GetFiles(name + "*" + outputFileFormat);
                foreach (var oldFile in oldFiles)
                {
                    var tempImages = Path.Combine(dirString, Path.GetFileNameWithoutExtension(oldFile.FullName) + "_files");
                    if (Directory.Exists(tempImages)) Directory.Delete(tempImages, true);
                    oldFile.Delete();
                }

                var doc = new Workbook();
                if (!string.IsNullOrEmpty(password)) doc.OpenPassword = password;
                doc.LoadFromFile(filename);
                doc.SaveToHtml(fPath);
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

                var doc = new Workbook();
                if (!string.IsNullOrEmpty(password)) doc.OpenPassword = password;
                doc.LoadFromFile(filename);
                var count = doc.Worksheets.Count;
                int width = 0, height = 0;
                var images = new Image[count];
                for (var i = 0; i < count; i++)
                {
                    var stream = doc.Worksheets[i].ToImage(0, 0, 0, 0);
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

    public interface IExcelTools
    {
        /// <summary>
        /// 导出 Excel 文档(*.xls,*.xlsx)
        /// </summary>
        /// <param name="templateFile">Excel模板文档</param>
        /// <param name="saveFileName">导出文件路径</param>
        /// <param name="list">数据列表</param>
        /// <param name="columnHeaders">列名关系{"A1","Hashtable:Key1"}</param>
        /// <param name="copyStyles">复制数据行第一行样式</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        void ExportWithList(string templateFile, string saveFileName, IEnumerable<Hashtable> list, Dictionary<string, string> columnHeaders, bool copyStyles = true, string password = null, bool readOnlyProtect = false);

        /// <summary>
        /// 导出 Excel 文档(*.xls,*.xlsx)
        /// </summary>
        /// <param name="templateFile">Excel模板文档</param>
        /// <param name="outputStream">输出字节流</param>
        /// <param name="list">数据列表</param>
        /// <param name="columnHeaders">列名关系{"A1","Hashtable:Key1"}</param>
        /// <param name="copyStyles">复制数据行第一行样式</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        void ExportWithList(string templateFile, Stream outputStream, IEnumerable<Hashtable> list, Dictionary<string, string> columnHeaders, bool copyStyles = true, string password = null, bool readOnlyProtect = false);

        /// <summary>
        /// 导出 Excel 文档(*.xls,*.xlsx)
        /// </summary>
        /// <param name="saveFileName">导出文件路径</param>
        /// <param name="dataTable">数据列表</param>
        /// <param name="columnHeaders">包含列名</param>
        /// <param name="firstRow">第一行</param>
        /// <param name="firstColumn">第一列</param>
        /// <param name="action">其它处理</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        void ExportWithDataTable(string saveFileName, DataTable dataTable, bool columnHeaders = true, int firstRow = 1, int firstColumn = 1, Action<Worksheet> action = null, string password = null, bool readOnlyProtect = false);

        /// <summary>
        /// 导出 Excel 文档(*.xls,*.xlsx)
        /// </summary>
        /// <param name="outputStream">输出字节流</param>
        /// <param name="dataTable">数据列表</param>
        /// <param name="columnHeaders">包含列名</param>
        /// <param name="firstRow">第一行</param>
        /// <param name="firstColumn">第一列</param>
        /// <param name="action">其它处理</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        void ExportWithDataTable(Stream outputStream, DataTable dataTable, bool columnHeaders = true, int firstRow = 1, int firstColumn = 1, Action<Worksheet> action = null, string password = null, bool readOnlyProtect = false);
    }
}
