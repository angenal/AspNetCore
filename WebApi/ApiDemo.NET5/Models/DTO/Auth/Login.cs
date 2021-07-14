using System.ComponentModel.DataAnnotations;

namespace ApiDemo.NET5.Models.DTO.Auth
{
    /// <summary>
    ///
    /// </summary>
    public class LoginInputDto
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Required]
        [RegularExpression(@"^[\w\-\.]{3,30}$", ErrorMessage = "wrong username format")]
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^[^\u4e00-\u9fa5]{8,30}$", ErrorMessage = "wrong password format")]
        public string Password { get; set; }
    }
}
