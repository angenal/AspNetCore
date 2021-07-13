using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiDemo.NET5.Models.DTO.Office
{
    public class ExcelExportDataInputDto
    {
        /// <summary>
        /// 数据列表
        /// </summary>
        [Required]
        public List<ExcelExportDataItemDto> Data { get; set; }
        /// <summary>
        /// 导出文件名
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// 是否使用模板 template.xlsx
        /// </summary>
        public bool Template { get; set; }
    }
    public class ExcelExportDataItemDto
    {
        public string NO { get; set; }
        public string Name { get; set; }
        public string Sex { get; set; }
        public string Nation { get; set; }
        public string Phone { get; set; }
        public string IdCard { get; set; }
        public string Memo { get; set; }
    }
}
