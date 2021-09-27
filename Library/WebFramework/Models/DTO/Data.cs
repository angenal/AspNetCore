using System.ComponentModel.DataAnnotations;

namespace WebFramework.Models.DTO
{
    /// <summary>
    /// 文本编码输入
    /// </summary>
    public class EncodeTextInputDto
    {
        /// <summary>
        /// 文本
        /// </summary>
        [Display(Name = "文本")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "文本编码最多255个字符")]
        public string Text { get; set; }
    }
    /// <summary>
    /// 文本编码输出
    /// </summary>
    public class EncodeTextOutputDto
    {
        /// <summary>
        /// 编码后的文本
        /// </summary>
        public string Text { get; set; }
    }
    /// <summary>
    /// 密钥对
    /// </summary>
    public class EncryptKeyPairInputDto
    {
        /// <summary>
        /// 密钥
        /// </summary>
        [Display(Name = "密钥")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "密钥为16个字符")]
        public string Key { get; set; }
        /// <summary>
        /// 向量
        /// </summary>
        [Display(Name = "向量")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "向量为16个字符")]
        public string Iv { get; set; }
    }
    /// <summary>
    /// 文本加密输入
    /// </summary>
    public class EncryptTextInputDto
    {
        /// <summary>
        /// 文本
        /// </summary>
        [Display(Name = "文本")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "文本编码最多255个字符")]
        public string Text { get; set; }
        /// <summary>
        /// 密钥对
        /// </summary>
        public EncryptKeyPairInputDto Keys { get; set; }
    }
    /// <summary>
    /// 文本解密输入
    /// </summary>
    public class DecryptTextInputDto
    {
        /// <summary>
        /// 加密文本
        /// </summary>
        [Display(Name = "文本")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "文本编码最多255个字符")]
        public string Text { get; set; }
        /// <summary>
        /// 密钥对
        /// </summary>
        public EncryptKeyPairInputDto Keys { get; set; }
    }
    /// <summary>
    /// 密钥对
    /// </summary>
    public class EncryptKeyPair1InputDto
    {
        /// <summary>
        /// 密钥
        /// </summary>
        [Display(Name = "密钥")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(32, MinimumLength = 32, ErrorMessage = "密钥为32个字符")]
        public string Key { get; set; }
        /// <summary>
        /// 随机值
        /// </summary>
        [Display(Name = "随机值")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(24, MinimumLength = 24, ErrorMessage = "随机值为24个字符")]
        public string Nonce { get; set; }
    }
    /// <summary>
    /// 文本加密输入
    /// </summary>
    public class EncryptText1InputDto
    {
        /// <summary>
        /// 文本
        /// </summary>
        [Display(Name = "文本")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "文本编码最多255个字符")]
        public string Text { get; set; }
        /// <summary>
        /// 密钥对
        /// </summary>
        [Required]
        public EncryptKeyPair1InputDto Keys { get; set; }
    }
    /// <summary>
    /// 文本解密输入
    /// </summary>
    public class DecryptText1InputDto
    {
        /// <summary>
        /// 加密文本
        /// </summary>
        [Display(Name = "文本")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "文本编码最多255个字符")]
        public string Text { get; set; }
        /// <summary>
        /// 密钥对
        /// </summary>
        [Required]
        public EncryptKeyPair1InputDto Keys { get; set; }
    }
    /// <summary>
    /// 密钥对
    /// </summary>
    public class EncryptKeyPair2InputDto
    {
        /// <summary>
        /// 密钥
        /// </summary>
        [Display(Name = "密钥")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(32, MinimumLength = 32, ErrorMessage = "密钥为32个字符")]
        public string Key { get; set; }
        /// <summary>
        /// 随机值
        /// </summary>
        [Display(Name = "随机值")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "随机值为8个字符")]
        public string Nonce { get; set; }
    }
    /// <summary>
    /// 文本加密输入
    /// </summary>
    public class EncryptText2InputDto
    {
        /// <summary>
        /// 文本
        /// </summary>
        [Display(Name = "文本")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "文本编码最多255个字符")]
        public string Text { get; set; }
        /// <summary>
        /// 密钥对
        /// </summary>
        [Required]
        public EncryptKeyPair2InputDto Keys { get; set; }
    }
    /// <summary>
    /// 文本解密输入
    /// </summary>
    public class DecryptText2InputDto
    {
        /// <summary>
        /// 加密文本
        /// </summary>
        [Display(Name = "文本")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "文本编码最多255个字符")]
        public string Text { get; set; }
        /// <summary>
        /// 密钥对
        /// </summary>
        [Required]
        public EncryptKeyPair2InputDto Keys { get; set; }
    }
    /// <summary>
    /// 密钥对
    /// </summary>
    public class EncryptKeyPair3InputDto
    {
        /// <summary>
        /// 密钥
        /// </summary>
        [Display(Name = "密钥")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(32, MinimumLength = 32, ErrorMessage = "密钥为32个字符")]
        public string Key { get; set; }
        /// <summary>
        /// 随机值
        /// </summary>
        [Display(Name = "随机值")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "随机值为12个字符")]
        public string Nonce { get; set; }
    }
    /// <summary>
    /// 文本加密输入
    /// </summary>
    public class EncryptText3InputDto
    {
        /// <summary>
        /// 文本
        /// </summary>
        [Display(Name = "文本")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "文本编码最多255个字符")]
        public string Text { get; set; }
        /// <summary>
        /// 密钥对
        /// </summary>
        [Required]
        public EncryptKeyPair3InputDto Keys { get; set; }
    }
    /// <summary>
    /// 文本解密输入
    /// </summary>
    public class DecryptText3InputDto
    {
        /// <summary>
        /// 加密文本
        /// </summary>
        [Display(Name = "文本")]
        [Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "文本编码最多255个字符")]
        public string Text { get; set; }
        /// <summary>
        /// 密钥对
        /// </summary>
        [Required]
        public EncryptKeyPair3InputDto Keys { get; set; }
    }
}
