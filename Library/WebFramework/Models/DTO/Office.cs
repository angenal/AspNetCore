using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebFramework.Models.DTO
{
    /// <summary>
    ///
    /// </summary>
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
    /// <summary>
    ///
    /// </summary>
    public class ExcelExportDataItemDto
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string NO { get; set; }
        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        public string Nation { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 身份证
        /// </summary>
        public string IdCard { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }
    }
}
