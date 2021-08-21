using System.ComponentModel.DataAnnotations;

namespace WebFramework.Models.DTO
{
    /// <summary>
    /// 上传文件结果
    /// </summary>
    public class UploadFileOutputDto
    {
        /// <summary>
        /// the form field name.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// the file name.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// the raw Content-Type header of the uploaded file.
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// the file length in bytes.
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// Output File Path.
        /// </summary>
        public string Path { get; set; }
    }


    /// <summary>
    /// PDF数字签名 by SignLib
    /// </summary>
    public class PdfFileSignInputDto
    {
        /// <summary>
        /// 未签名文件的路径
        /// </summary>
        [Required]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "文件路径错误")]
        public string Path { get; set; }
    }


    /// <summary>
    /// 生成签名密钥 by Minisign
    /// </summary>
    public class MinisignGenerateKeyInputDto : MinisignKeyInputDto
    {
        /// <summary>
        /// 重新生成
        /// </summary>
        public bool? Renew { get; set; }
    }
    /// <summary>
    /// 签名密钥 by Minisign
    /// </summary>
    public class MinisignKeyInputDto
    {
        /// <summary>
        /// 安全密钥
        /// </summary>
        [Required]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "安全密钥字符长度为8～64")]
        public string KeyPass { get; set; }
        /// <summary>
        /// 密钥文件名;不包括文件扩展名,默认minisign
        /// </summary>
        public string KeyFile { get; set; } = "minisign";
    }
    /// <summary>
    /// 签名密钥 by Minisign
    /// </summary>
    public class MinisignKeyOutputDto : MinisignKeyInputDto
    {
        /// <summary>
        /// 安全密钥Id
        /// </summary>
        [Required]
        public string KeyId { get; set; }
    }
    /// <summary>
    /// 文件签名 by Minisign
    /// </summary>
    public class MinisignFileSignInputDto : MinisignKeyOutputDto
    {
        /// <summary>
        /// 未签名文件的路径
        /// </summary>
        [Required]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "文件路径错误")]
        public string Path { get; set; }
    }
    /// <summary>
    /// 文件签名结果
    /// </summary>
    public class FileSignOutputDto
    {
        /// <summary>
        /// 已签名文件的路径
        /// </summary>
        public string Path { get; set; }
    }

}
