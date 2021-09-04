using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebFramework.SignalR;

namespace WebFramework.Models.DTO
{
    /// <summary></summary>
    public class MessageUserSelectInputDto
    {
        /// <summary>
        /// 用户Id from Sid
        /// </summary>
        [Required]
        [RegularExpression(@"^[\w\-\.]{3,36}$", ErrorMessage = "wrong user id format")]
        public string UserId { get; set; }
    }
    /// <summary></summary>
    public class MessageUsersSelectInputDto
    {
        /// <summary>
        /// 用户Id from Sid
        /// </summary>
        [Required]
        public List<string> UserId { get; set; }
    }
    /// <summary></summary>
    public class MessageUsersListInputDto
    {
        /// <summary>
        /// 消息群
        /// </summary>
        [Required]
        [RegularExpression(@"^[\w\-\.]{3,36}$", ErrorMessage = "wrong group name format")]
        public string GroupName { get; set; }
        /// <summary>
        /// 获取数量
        /// </summary>
        public int Size { get; set; } = 20;
    }
    /// <summary></summary>
    public class MessageListInputDto
    {
        /// <summary>
        /// 消息群
        /// </summary>
        [Required]
        [RegularExpression(@"^[\w\-\.]{3,36}$", ErrorMessage = "wrong group name format")]
        public string GroupName { get; set; }
        /// <summary>
        /// 获取数量
        /// </summary>
        public int Size { get; set; } = 20;
    }
    /// <summary></summary>
    public class MessageInputDto
    {
        /// <summary>
        /// 消息群用户
        /// </summary>
        [Required]
        public List<RoomUser> ToUser { get; set; }

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
    /// <summary></summary>
    public class MessagesInputDto
    {
        /// <summary>
        /// 消息群
        /// </summary>
        [Required]
        [RegularExpression(@"^[\w\-\.]{3,36}$", ErrorMessage = "wrong group name format")]
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
