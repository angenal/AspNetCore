using System.ComponentModel.DataAnnotations;

namespace WebSwaggerDemo.NET5.V2.Models
{
    /// <summary>
    /// 人
    /// </summary>
    public class Person
    {
        /// <summary>
        /// 人
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 姓
        /// </summary>
        [Required]
        [StringLength(25)]
        public string FirstName { get; set; }

        /// <summary>
        /// 名
        /// </summary>
        [Required]
        [StringLength(25)]
        public string LastName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
    }
}
