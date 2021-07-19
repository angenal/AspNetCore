using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebCore;
using WebCore.Attributes;

namespace WebFramework
{
    /// <summary>
    /// 上传文件
    /// </summary>
    public class PostFormFile<T> where T : class
    {

        /// <summary>
        /// 获取文本文件(UTF8 Encoding)用于将文件内容保存到数据库
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="file"></param>
        /// <param name="fileMaxLength"></param>
        /// <returns>文件内容</returns>
        public static async Task<string> GetUploadedTextFile(ModelStateDictionary modelState, IFormFile file, int maximumLength, bool autoConvertToUtf8 = false)
        {
            string fContent = "";
            var (Checked, kErr, vErr) = CheckUploadedFile(modelState, file, FileSomeType.Text, new Size[] { Size.Zero, new Size(maximumLength) }, new string[] { "text/plain" }, "文本文件");
            if (Checked)
            {
                try
                {
                    if (!autoConvertToUtf8)
                    {
                        using (var reader = new StreamReader(file.OpenReadStream(),
                            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                            detectEncodingFromByteOrderMarks: true))
                        {
                            fContent = await reader.ReadToEndAsync();
                        }
                    }
                    else
                    {
                        Encoding encoding = Encoding.Default;
                        using (var reader = new StreamReader(file.OpenReadStream(), Encoding.Default, true))
                        {
                            fContent = await reader.ReadToEndAsync();
                            encoding = reader.CurrentEncoding;
                            reader.Close();
                        }
                        if (encoding != Encoding.UTF8)
                        {
                            byte[] encBytes = encoding.GetBytes(fContent);
                            byte[] utf8Bytes = Encoding.Convert(encoding, Encoding.UTF8, encBytes);
                            fContent = Encoding.UTF8.GetString(utf8Bytes);
                        }
                    }
                }
                catch (Exception e)
                {
                    modelState.AddModelError(kErr, $"{vErr}上传失败: {e.Message}");
                }
            }
            return fContent;
        }

        /// <summary>
        /// 获取文本文件(UTF8 Encoding)用于将文件保存到磁盘
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="file"></param>
        /// <param name="filePath">指定的位置具有写入权限</param>
        /// <param name="fileMaxLength"></param>
        /// <returns></returns>
        public static async Task<bool> SaveUploadedTextFile(ModelStateDictionary modelState, IFormFile file, string filePath, int maximumLength)
        {
            var (Checked, kErr, vErr) = CheckUploadedFile(modelState, file, FileSomeType.Text, new Size[] { Size.Zero, new Size(maximumLength) }, new string[] { "text/plain" }, "文本文件");
            if (Checked)
            {
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    modelState.AddModelError(kErr, $"{vErr}上传失败: {e.Message}");
                }
            }
            return false;
        }

        /// <summary>
        /// 获取上传图片并保存
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="file"></param>
        /// <param name="filePath">指定的位置具有写入权限</param>
        /// <param name="sizes"></param>
        /// <returns></returns>
        public static async Task<bool> SaveUploadedImageFile(ModelStateDictionary modelState, IFormFile file, string filePath, Size[] sizes)
        {
            var (Checked, kErr, vErr) = CheckUploadedFile(modelState, file, FileSomeType.Image, sizes, new string[] { "image/jpeg", "image/png" }, "jpg|png图片");
            if (Checked)
            {
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    modelState.AddModelError(file.Name, $"{vErr}上传失败: {e.Message}");
                }
            }
            return Checked;
        }


        internal static (bool Checked, string kErr, string vErr) CheckUploadedFile(ModelStateDictionary modelState, IFormFile file, FileSomeType fileType, Size[] sizes, string[] fileTypes, string fileTypeName)
        {
            bool _Checked = modelState.IsValid;
            string _kErr = file.Name, _vErr = "";
            if (_Checked)
            {
                string sTitle = Attr<T>.DisplayNameFor(file.Name.Substring(file.Name.LastIndexOf('.') + 1))?.Name ?? "文件",
                    fileName = WebUtility.HtmlEncode(Path.GetFileName(file.FileName));
                _vErr = $"{sTitle} - {fileName}";

                //检查文件类型
                if (!fileTypes.Any(o => o.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase)))
                {
                    modelState.AddModelError(_kErr, $"{_vErr}必须是{fileTypeName}");
                    _Checked = false;
                }

                //检查文件大小
                if (_Checked && file.Length == 0)
                {
                    modelState.AddModelError(_kErr, $"{_vErr}不能为空");
                    _Checked = false;
                }
                if (_Checked && sizes != null && sizes.Length == 2)
                {
                    SizeUnit sizeUnit = SizeUnit.Bytes;
                    long minL = sizes[0].GetValue(sizeUnit), maxL = sizes[1].GetValue(sizeUnit);
                    if (0 < minL && file.Length < minL)
                    {
                        if (fileType == FileSomeType.Text)
                            modelState.AddModelError(_kErr, $"{_vErr}不能少于{minL}字");
                        else
                            modelState.AddModelError(_kErr, $"{_vErr}不能小于{new Size(minL, sizeUnit).ToString()}");
                        _Checked = false;
                    }
                    else if (0 < maxL && file.Length > maxL)
                    {
                        if (fileType == FileSomeType.Text)
                            modelState.AddModelError(_kErr, $"{_vErr}不能多于{maxL}字");
                        else
                            modelState.AddModelError(_kErr, $"{_vErr}不能大于{new Size(maxL, sizeUnit).ToString()}");
                        _Checked = false;
                    }
                }

                if (_Checked) { _vErr = ""; }
            }
            return (_Checked, _kErr, _vErr);
        }

    }
}
