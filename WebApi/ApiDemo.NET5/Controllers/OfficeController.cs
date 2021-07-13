using ApiDemo.NET5.Models.DTO.Office;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using WebFramework;
using WebInterface;

namespace ApiDemo.NET5.Controllers
{
    /// <summary>
    /// 办公文档
    /// </summary>
    [ApiController]
    //[ApiExplorerSettings(GroupName = "demo"), Display(Name = "演示系统", Description = "演示系统描述文字")]
    [ApiVersion("1.0")]
    [Route("api/[controller]/[action]")]
    //[Route("{culture:culture}/[controller]/[action]")]
    public class OfficeController : ApiController
    {
        private readonly IWebHostEnvironment Env;
        private readonly IExcelTools Excel;

        /// <summary>
        ///
        /// </summary>
        /// <param name="env"></param>
        /// <param name="excel"></param>
        public OfficeController(IWebHostEnvironment env, IExcelTools excel)
        {
            Env = env;
            Excel = excel;
        }


        #region api/Office/ExcelExport

        /// <summary>
        /// Excel Export with template.xlsx
        /// </summary>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult ExcelExport([FromBody] ExcelExportDataInputDto input)
        {
            try
            {
                var name = input.Filename ?? $"{nameof(ExcelExport)}-{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                if (name.ToLower().EndsWith(".xlsx")) name = name.Substring(0, name.Length - 5);

                if (!input.Template)
                {
                    var table = input.Data.ToDataTable();
                    return ExcelExportDataTable(table, name);
                }

                string dir = Env.ContentRootPath, templateFile = $"{dir}\\App_Data\\template.xlsx";
                var columnHeaders = new Dictionary<string, string>
                {
                    {"A1","NO"},{"B1","Name"},{"C1","Sex"},{"D1","Nation"},{"E1","Phone"},{"F1","IdCard"},{"G1","Memo"}
                };
                var content = new MemoryStream();
                var list = input.Data.ToHashtables();
                Excel.ExportWithList(templateFile, content, list, columnHeaders);
                content.Seek(0, SeekOrigin.Begin);
                return File(content, "application/octet-stream", $"{name}.xlsx");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        private ActionResult ExcelExportDataTable(DataTable table, string name)
        {
            try
            {
                var content = new MemoryStream();
                Excel.ExportWithDataTable(content, table, true, 1, 1, sheet =>
                {
                    sheet.DefaultRowHeight = 21;
                    sheet.Range.Style.HorizontalAlignment = Spire.Xls.HorizontalAlignType.Center;
                    sheet.Range.Style.VerticalAlignment = Spire.Xls.VerticalAlignType.Center;
                    sheet.FreezePanes(sheet.FirstRow + 1, sheet.FirstColumn); // 冻结首行

                    var firstRow = sheet.Range.Rows[0];
                    firstRow.AutoFitColumns();
                    firstRow.Style.HorizontalAlignment = Spire.Xls.HorizontalAlignType.Left;
                    var firstColumn = firstRow.Columns[0];
                    firstColumn.Style.HorizontalAlignment = Spire.Xls.HorizontalAlignType.Right;
                    var fontBold = sheet.Workbook.CreateFont();
                    fontBold.IsBold = true;
                    foreach (Spire.Xls.CellRange column in firstRow.Columns)
                    {
                        var richText = column.RichText;
                        var text = column.DisplayedText;
                        int index = text.IndexOf('＆'), length = text.Length;
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
                });
                content.Seek(0, SeekOrigin.Begin);

                return File(content, "application/octet-stream", $"{name}.xlsx");
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }
        }

        #endregion


        #region api/Office/Preview

        /// <summary>
        /// Generate HTML preview by Aspose
        /// </summary>
        /// <param name="filename">test.doc,xls,pptx,pdf</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult PreviewA(string filename)
        {
            string dir = Env.WebRootPath, files = "office/files", srcDoc = $"{files}/{filename}", srcPath = Path.Combine(dir, srcDoc);
            var doc = new FileInfo(srcPath);
            if (!doc.Exists) return Ok("");

            string name = doc.FullName.Md5(), salt = doc.LastWriteTimeHex(), ext = ".html";
            string saveDoc = $"{files}/temp/{name}{salt}{ext}", savePath = Path.Combine(dir, saveDoc), url = "/" + saveDoc;
            if (System.IO.File.Exists(savePath)) return Ok(url);

            bool tempImagesDelete = false;
            switch (doc.Extension.ToLower())
            {
                case ".doc":
                case ".docx":
                    var document = new Aspose.Words.Document(doc.FullName);
                    document.Save(savePath, Aspose.Words.SaveFormat.Html);
                    tempImagesDelete = true;
                    break;
                case ".xls":
                case ".xlsx":
                    var xls = new Aspose.Cells.Workbook(doc.FullName);
                    xls.Save(savePath, Aspose.Cells.SaveFormat.Html);
                    break;
                case ".ppt":
                case ".pptx":
                    var ppt = new Aspose.Slides.Presentation(doc.FullName);
                    ppt.Save(savePath, Aspose.Slides.Export.SaveFormat.Html);
                    break;
                case ".pdf":
                    string outputDirectory = Path.GetDirectoryName(savePath), uriString = $"/{files}/temp", password = null;
                    url = PdfTools.SaveToFile(doc, outputDirectory, ext, uriString, password, name, salt);
                    break;
            }

            var ts = new List<string>();
            int tl = salt.Length, len = tl + ext.Length;
            var t1 = savePath.Substring(savePath.Length - len, tl);
            dir = Path.GetDirectoryName(savePath);
            foreach (var file in Directory.GetFiles(dir, $"{name}*{ext}"))
            {
                var t0 = file.Substring(file.Length - len, tl);
                if (t0.Equals(t1)) continue;
                System.IO.File.Delete(file);
                ts.Add(t0);
            }
            if (!tempImagesDelete) return Ok(url);
            foreach (var t0 in ts) foreach (var file in Directory.GetFiles(dir, $"{name}{t0}.*.png")) System.IO.File.Delete(file);
            return Ok(url);
        }

        /// <summary>
        /// Generate HTML preview by Spire
        /// </summary>
        /// <param name="filename">test.doc,xls,pptx,pdf</param>
        /// <param name="format">html,png,pdf</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult PreviewS(string filename, string format = "html")
        {
            string dir = Env.WebRootPath, files = "office/files", srcDoc = $"{files}/{filename}", srcPath = Path.Combine(dir, srcDoc);
            var doc = new FileInfo(srcPath);
            if (!doc.Exists) return Ok("");

            string name = doc.FullName.Md5(), salt = doc.LastWriteTimeHex(), ext = "." + format;
            string saveDoc = $"{files}/temp/{name}{salt}{ext}", savePath = Path.Combine(dir, saveDoc), url = "/" + saveDoc;
            if (System.IO.File.Exists(savePath)) return Ok(url);

            string outputDirectory = Path.GetDirectoryName(savePath), uriString = $"/{files}/temp", password = null;
            switch (doc.Extension.ToLower())
            {
                case ".doc":
                case ".docx":
                    url = WordTools.SaveToFile(doc, outputDirectory, ext, uriString, password, name, salt);
                    break;
                case ".xls":
                case ".xlsx":
                    url = ExcelTools.SaveToFile(doc, outputDirectory, ext, uriString, password, name, salt);
                    break;
                case ".ppt":
                case ".pptx":
                    url = PptTools.SaveToFile(doc, outputDirectory, ext, uriString, password, name, salt);
                    break;
                case ".pdf":
                    url = PdfTools.SaveToFile(doc, outputDirectory, ext, uriString, password, name, salt);
                    break;
            }

            return Ok(url);
        }

        #endregion

    }
}
