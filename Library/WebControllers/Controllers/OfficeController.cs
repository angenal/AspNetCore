using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using WebCore;
using WebFramework;
using WebFramework.Models.DTO;
using WebInterface;

namespace WebControllers.Controllers
{
    /// <summary>
    /// 办公文档
    /// </summary>
    [ApiController]
    //[ApiExplorerSettings(GroupName = "demo"), Display(Name = "演示系统", Description = "演示系统描述文字")]
    //[ApiVersion("1.0")]
    [Route("api/[controller]/[action]")]
    //[Route("{culture:culture}/[controller]/[action]")]
    public class OfficeController : ApiController
    {
        private readonly IWebHostEnvironment env;
        private readonly IExcelTools excel;
        private readonly IWordTools word;
        private readonly IPptTools ppt;
        private readonly IPdfTools pdf;

        /// <summary></summary>
        public OfficeController(IWebHostEnvironment env, IExcelTools excel, IWordTools word, IPptTools ppt, IPdfTools pdf)
        {
            this.env = env;
            this.excel = excel;
            this.word = word;
            this.ppt = ppt;
            this.pdf = pdf;
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

                string dir = env.ContentRootPath, templateFile = $"{dir}\\App_Data\\template.xlsx";
                var columnHeaders = new Dictionary<string, string>
                {
                    {"A1","NO"},{"B1","Name"},{"C1","Sex"},{"D1","Nation"},{"E1","Phone"},{"F1","IdCard"},{"G1","Memo"}
                };
                var content = new MemoryStream();
                var list = input.Data.ToHashtables();
                excel.ExportWithList(templateFile, content, list, columnHeaders);
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
                excel.ExportWithDataTable(content, table, true, 1, 1);
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
        /// Generate HTML Preview
        /// </summary>
        /// <param name="filename">test.doc,xls,pptx,pdf</param>
        /// <param name="format">html,png,pdf</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult PreviewS(string filename, string format = "html")
        {
            string dir = env.WebRootPath, files = "office/files", srcDoc = $"{files}/{filename}", srcPath = Path.Combine(dir, srcDoc);
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
                    url = word.SaveToFile(doc, outputDirectory, ext, uriString, password, name, salt);
                    break;
                case ".xls":
                case ".xlsx":
                    url = excel.SaveToFile(doc, outputDirectory, ext, uriString, password, name, salt);
                    break;
                case ".ppt":
                case ".pptx":
                    url = ppt.SaveToFile(doc, outputDirectory, ext, uriString, password, name, salt);
                    break;
                case ".pdf":
                    url = pdf.SaveToFile(doc, outputDirectory, ext, uriString, password, name, salt);
                    break;
            }

            return Ok(url);
        }

        #endregion

    }
}
