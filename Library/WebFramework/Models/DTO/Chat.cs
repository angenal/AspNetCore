using System.ComponentModel.DataAnnotations;

namespace WebFramework.Models.DTO
{
    /// <summary>
    ///
    /// </summary>
    public class MessageListInputDto
    {
        /// <summary>
        /// 消息群
        /// </summary>
        [Required]
        [RegularExpression(@"^[\w\-\.]{3,30}$", ErrorMessage = "wrong group name format")]
        public string GroupName { get; set; }
        /// <summary>
        /// 获取数量
        /// </summary>
        public int Size { get; set; } = 20;
    }
    /// <summary>
    ///
    /// </summary>
    public class MessageInputDto
    {
        /// <summary>
        /// 消息群
        /// </summary>
        [Required]
        [RegularExpression(@"^[\w\-\.]{3,30}$", ErrorMessage = "wrong group name format")]
        public string GroupName { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; }
        /// <summary>
        /// 消息来源
        /// </summary>
        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string From { get; set; }
    }
}
