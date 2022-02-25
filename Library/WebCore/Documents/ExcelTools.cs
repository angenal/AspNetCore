using Spire.Xls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using WebInterface;

namespace WebCore.Documents
{
    public class ExcelTools : IExcelTools
    {
        /// <summary>
        /// 设置表格样式
        /// </summary>
        public void SetWorksheetFormat(Worksheet sheet, bool freezeFirstRow = true, bool isBoldFirstRow = true, string notBoldStartText = "＆")
        {
            sheet.DefaultRowHeight = 21;
            sheet.Range.Style.HorizontalAlignment = HorizontalAlignType.Center;
            sheet.Range.Style.VerticalAlignment = VerticalAlignType.Center;
            if (freezeFirstRow) sheet.FreezePanes(sheet.FirstRow + 1, sheet.FirstColumn); // 冻结首行

            var firstRow = sheet.Range.Rows[0];
            firstRow.AutoFitColumns();
            firstRow.Style.HorizontalAlignment = HorizontalAlignType.Left;
            var firstColumn = firstRow.Columns[0];
            firstColumn.Style.HorizontalAlignment = HorizontalAlignType.Right;

            if (isBoldFirstRow)
            {
                var fontBold = sheet.Workbook.CreateFont();
                fontBold.IsBold = true;
                foreach (CellRange column in firstRow.Columns)
                {
                    var richText = column.RichText;
                    var text = column.DisplayedText;
                    int index = text.IndexOfAny(notBoldStartText.ToCharArray()), length = text.Length;
                    if (index < 1)
                    {
                        richText.Text = text;
                        richText.SetFont(0, text.Length, fontBold);
                    }
                    else
                    {
                        richText.Text = text.Substring(0, index) + (index == length - 1 ? "" : text.Substring(index + 1));
                        richText.SetFont(0, index - 1, fontBold);
                    }
                }
            }
        }

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
        public void ExportWithDataTable(string saveFileName, DataTable dataTable, bool columnHeaders = true, int firstRow = 1, int firstColumn = 1, string password = null, bool readOnlyProtect = false)
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
                // 设置表格样式
                SetWorksheetFormat(sheet);
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
        public void ExportWithDataTable(Stream outputStream, DataTable dataTable, bool columnHeaders = true, int firstRow = 1, int firstColumn = 1, string password = null, bool readOnlyProtect = false)
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
                // 设置表格样式
                SetWorksheetFormat(sheet);
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
        /// 另存 Excel 文档(*.pdf,*.html,*.png)
        /// </summary>
        /// <param name="source">来源文件(*.xls,*.xlsx)</param>
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

                var doc = new Workbook();
                if (!string.IsNullOrEmpty(password)) doc.OpenPassword = password;
                doc.LoadFromFile(filename);
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

            if (outputFileFormat == "png" || outputFileFormat == ".png")
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


        /// <summary>
        /// Excel文件转换为DataSet
        /// </summary>
        /// <param name="path">Excel文件路径</param>
        /// <param name="columnsCount">表格列数限制</param>
        /// <param name="hasTitle">是否有表头</param>
        /// <param name="columnsName">表格列名列表</param>
        /// <param name="skipFirstColumnEmptyRow">跳过第一列为空的数据</param>
        /// <param name="getBackgroundColor">获取背景颜色RGB格式: #000000</param>
        /// <param name="trimText">清除单元格中的空白字符</param>
        public DataSet ImportDataSet(string path, int[] columnsCount, bool[] hasTitle, string[][] columnsName, bool[] skipFirstColumnEmptyRow, bool[] getBackgroundColor, bool[] trimText)
        {
            using (var workbook = new Workbook())
            {
                workbook.LoadFromFile(path);
                var ds = new DataSet();
                var count = workbook.Worksheets.Count;
                if (count == 0) return ds;
                for (var i = 0; i < count; i++)
                {
                    var sheet = workbook.Worksheets[i];
                    var table = SheetToDataTable(sheet, columnsCount[i], hasTitle[i], columnsName[i], skipFirstColumnEmptyRow[i], getBackgroundColor[i], trimText[i]);
                    ds.Tables.Add(table);
                }
                return ds;
            }
        }

        /// <summary>
        /// Excel文件流转换为DataSet
        /// </summary>
        /// <param name="stream">Excel文件流</param>
        /// <param name="worksheetIndex">工作表索引</param>
        /// <param name="columnsCount">表格列数限制</param>
        /// <param name="hasTitle">是否有表头</param>
        /// <param name="columnsName">表格列名列表</param>
        /// <param name="skipFirstColumnEmptyRow">跳过第一列为空的数据</param>
        /// <param name="getBackgroundColor">获取背景颜色RGB格式: #000000</param>
        /// <param name="trimText">清除单元格中的空白字符</param>
        public DataSet ImportDataSet(Stream stream, int[] columnsCount, bool[] hasTitle, string[][] columnsName, bool[] skipFirstColumnEmptyRow, bool[] getBackgroundColor, bool[] trimText)
        {
            using (var workbook = new Workbook())
            {
                workbook.LoadFromStream(stream);
                var ds = new DataSet();
                var count = workbook.Worksheets.Count;
                if (count == 0) return ds;
                for (var i = 0; i < count; i++)
                {
                    var sheet = workbook.Worksheets[i];
                    var table = SheetToDataTable(sheet, columnsCount[i], hasTitle[i], columnsName[i], skipFirstColumnEmptyRow[i], getBackgroundColor[i], trimText[i]);
                    ds.Tables.Add(table);
                }
                return ds;
            }
        }

        /// <summary>
        /// Excel文件转换为DataTable
        /// </summary>
        /// <param name="path">Excel文件路径</param>
        /// <param name="worksheetIndex">工作表索引</param>
        /// <param name="columnsCount">表格列数限制</param>
        /// <param name="hasTitle">是否有表头</param>
        /// <param name="columnsName">表格列名列表</param>
        /// <param name="skipFirstColumnEmptyRow">跳过第一列为空的数据</param>
        /// <param name="getBackgroundColor">获取背景颜色RGB格式: #000000</param>
        /// <param name="trimText">清除单元格中的空白字符</param>
        public DataTable ImportDataTable(string path, int worksheetIndex = 0, int columnsCount = 0, bool hasTitle = true, string[] columnsName = null, bool skipFirstColumnEmptyRow = true, bool getBackgroundColor = false, bool trimText = false)
        {
            using (var workbook = new Workbook())
            {
                workbook.LoadFromFile(path);
                if (worksheetIndex >= workbook.Worksheets.Count) return new DataTable();
                var sheet = workbook.Worksheets[worksheetIndex];
                return SheetToDataTable(sheet, columnsCount, hasTitle, columnsName, skipFirstColumnEmptyRow, getBackgroundColor, trimText);
            }
        }

        /// <summary>
        /// Excel文件流转换为DataTable
        /// </summary>
        /// <param name="stream">Excel文件流</param>
        /// <param name="worksheetIndex">工作表索引</param>
        /// <param name="columnsCount">表格列数限制</param>
        /// <param name="hasTitle">是否有表头</param>
        /// <param name="columnsName">表格列名列表</param>
        /// <param name="skipFirstColumnEmptyRow">跳过第一列为空的数据</param>
        /// <param name="getBackgroundColor">获取背景颜色RGB格式: #000000</param>
        /// <param name="trimText">清除单元格中的空白字符</param>
        public DataTable ImportDataTable(Stream stream, int worksheetIndex = 0, int columnsCount = 0, bool hasTitle = true, string[] columnsName = null, bool skipFirstColumnEmptyRow = true, bool getBackgroundColor = false, bool trimText = false)
        {
            using (var workbook = new Workbook())
            {
                workbook.LoadFromStream(stream);
                if (worksheetIndex >= workbook.Worksheets.Count) return new DataTable();
                var sheet = workbook.Worksheets[worksheetIndex];
                return SheetToDataTable(sheet, columnsCount, hasTitle, columnsName, skipFirstColumnEmptyRow, getBackgroundColor, trimText);
            }
        }

        static DataTable SheetToDataTable(Worksheet sheet, int columnsCount = 0, bool hasTitle = true, string[] columnsName = null, bool skipFirstColumnEmptyRow = true, bool getBackgroundColor = false, bool trimText = false)
        {
            int iRowCount = sheet.Rows.Length;
            int iColCount = sheet.Columns.Length;
            if (0 < columnsCount && columnsCount <= iColCount) iColCount = columnsCount;
            var dt = new DataTable();
            // 生成表头
            if (columnsName == null || columnsName.Length == 0)
            {
                for (int i = 0; i < iColCount; i++)
                {
                    var columnName = "column" + i;
                    if (hasTitle)
                    {
                        var text = sheet.Range[1, i + 1].DisplayedText;
                        if (!string.IsNullOrEmpty(text)) columnName = trimText ? text.Trim() : text;
                    }
                    while (dt.Columns.Contains(columnName)) columnName += "_1";
                    dt.Columns.Add(new DataColumn(columnName));
                }
            }
            else
            {
                for (int i = 0; i < iColCount; i++)
                {
                    var columnName = columnsName[i];
                    while (dt.Columns.Contains(columnName)) columnName += "_1";
                    dt.Columns.Add(new DataColumn(columnName));
                }
            }
            // 生成数据
            for (int iRow = hasTitle ? 2 : 1; iRow <= iRowCount; iRow++)
            {
                if (skipFirstColumnEmptyRow && string.IsNullOrEmpty(sheet.Range[iRow, 1].DisplayedText)) continue;
                DataRow dr = dt.NewRow();
                for (int iCol = 1; iCol <= iColCount; iCol++)
                {
                    var text = sheet.Range[iRow, iCol].DisplayedText ?? "";
                    if (trimText) text = text.Trim();
                    if (getBackgroundColor)
                    {
                        var color = sheet.Range[iRow, iCol].Style.Color;
                        text += "#" + color.R.ToString("x2") + color.G.ToString("x2") + color.B.ToString("x2");
                    }
                    dr[iCol - 1] = text;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}
