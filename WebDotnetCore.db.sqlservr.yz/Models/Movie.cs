using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebDotnetCore.db.sqlservr.yz.Models
{
    /// <summary>
    /// 电影
    /// </summary>
    public class Movie
    {
        [Display(Name = "ID", Description = "主键ID")]
        [Required]
        public int ID { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Display(Name = "标题", Description = "电影标题")]
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "标题必填，字数：2～50")]
        [RegularExpression(@"^\S*.+\S*$", ErrorMessage = "电影标题不能是空白字符")]
        public string Title { get; set; }

        /// <summary>
        /// 发行日期
        /// </summary>
        [Display(Name = "发行日期", Description = "电影发行日期")]
        [Range(typeof(DateTime), "1900-01-01", "2020-01-01", ErrorMessage = "发行日期，范围：1900-01-01～2020-01-01")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "-")]
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        [Display(Name = "类型", Description = "电影类型")]
        [Required]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "标题必填，字数：2～30")]
        [RegularExpression(@"^\S*.+\S*$", ErrorMessage = "电影类型不能是空白字符")]
        public string Genre { get; set; }

        /// <summary>
        /// 定价
        /// </summary>
        [Display(Name = "定价", Description = "电影的定价")]
        [Required]
        [Range(1, 1000, ErrorMessage = "定价必填，范围：1～1000")]
        [DataType(DataType.Currency), DisplayFormat(DataFormatString = "{0:f2}")]
        public double Price { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        [Display(Name = "评分"), DisplayFormat(DataFormatString = "{0:N1}")]
        [Required]
        [Range(0, 100, ErrorMessage = "评分必填，范围：1～100")]
        public int Score { get; set; }
    }
}
