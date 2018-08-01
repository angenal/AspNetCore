using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebCore;
using WebCore.Attributes;

namespace WebDotnetCore.db.sqlservr.yz.Models
{
    /// <summary>
    /// 电影计划列表
    /// </summary>
    public class MovieSchedule
    {
        [Display(Name = "ID", Description = "主键ID")]
        [Required]
        public int ID { get; set; }

        [Display(Name = "电影ID", Description = "电影-主键ID")]
        [Required]
        public int MovieID { get; set; }

        /// <summary>
        /// 计划标题
        /// </summary>
        [Display(Name = "计划标题", Description = "计划标题")]
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "标题必填，字数：2～50")]
        [RegularExpression(@"^\S*.+\S*$", ErrorMessage = "计划标题不能是空白字符")]
        public string Title { get; set; }

        /// <summary>
        /// 上传计划内容
        /// </summary>
        [Display(Name = "上传计划内容(txt)", Description = "上传计划内容(文本文件)")]
        [Required]
        [StringLength(4000, ErrorMessage = "计划内容不能超过4000字")]
        public string UploadSchedule { get; set; }

        /// <summary>
        /// 上传图片
        /// </summary>
        [Display(Name = "上传图片(jpg)", Description = "上传图片(jpg)")]
        [StringLength(200, ErrorMessage = "上传图片不能超过2M"), FileSize(2)]
        public string UploadImage { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        [Display(Name = "上传时间"), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime UploadDT { get; set; }

    }
}
