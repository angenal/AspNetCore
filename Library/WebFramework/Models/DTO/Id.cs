using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebFramework.Models.DTO
{
    /// <summary>
    ///
    /// </summary>
    public class IdInput
    {
        /// <summary>
        /// 编号(可选)
        /// </summary>
        [Display(Name = "编号")]
        public string Id { get; set; }
    }
    /// <summary>
    ///
    /// </summary>
    public class IdInputDto
    {
        /// <summary>
        /// 编号
        /// </summary>
        [Display(Name = "编号")]
        [Required(ErrorMessage = "{0} 为必填项")]
        public string Id { get; set; }
    }
    /// <summary>
    ///
    /// </summary>
    public class IdOutputDto
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string Id { get; set; }
    }
    /// <summary>
    ///
    /// </summary>
    public class IdxOutputDto
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 附加信息
        /// </summary>
        public object X { get; set; }
    }
    /// <summary>
    ///
    /// </summary>
    public class IdListInputDto
    {
        /// <summary>
        /// 编号列表
        /// </summary>
        [Display(Name = "编号")]
        [Required(ErrorMessage = "{0} 为必填项")]
        public List<string> Id { get; set; }
    }
}
