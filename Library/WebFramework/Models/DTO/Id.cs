using System.ComponentModel.DataAnnotations;

namespace WebFramework.Models.DTO
{
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
}
