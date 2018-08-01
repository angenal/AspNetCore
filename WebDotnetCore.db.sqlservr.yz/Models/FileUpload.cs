using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebDotnetCore.db.sqlservr.yz.Models
{
    /// <summary>
    /// 上传文件
    /// </summary>
    public class FileUpload
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Display(Name = "标题")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "标题错误")]
        public string Title { get; set; }

        /// <summary>
        /// 文本文件
        /// </summary>
        [Display(Name = "文本文件")]
        public IFormFile TxtFile { get; set; }

        /// <summary>
        /// 图片文件
        /// </summary>
        [Display(Name = "图片文件")]
        public IFormFile ImgFile { get; set; }
    }
}
