using System;
using System.ComponentModel.DataAnnotations;

namespace ApiDemo.NET5.Models.DTO.User
{
    /// <summary>
    ///
    /// </summary>
    public class RegisterInputDto
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

        /// <summary>
        /// 邮箱
        /// </summary>
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        //[DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^1[0-9]{10}$", ErrorMessage = "wrong phone number format")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 身份证
        /// </summary>
        [RegularExpression(@"^[0-9]{18}$", ErrorMessage = "wrong ID card format")]
        public string IdCard { get; set; }
    }
    /// <summary>
    ///
    /// </summary>
    public class RegisterOutputDto
    {
        /// <summary>
        ///
        /// </summary>
        public Guid Id { get; set; }
    }
}
