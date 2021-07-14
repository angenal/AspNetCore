using System.Collections.Generic;
using System.IO;

namespace WebInterface
{
    public interface IWordTools
    {
        /// <summary>
        /// 导出 Word 文档(*.doc,*.docx)替换模板文档的书签
        /// </summary>
        /// <param name="templateFile">Word模板文档</param>
        /// <param name="saveFileName">导出文件路径</param>
        /// <param name="dictBookMark">数据字典</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        void ExportWithBookMark(string templateFile, string saveFileName, Dictionary<string, string> dictBookMark, string password = null, bool readOnlyProtect = false);

        /// <summary>
        /// 保护 Word 文档(*.doc,*.docx)
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <param name="password">文件密码</param>
        /// <param name="saveFileName">加密文件路径</param>
        /// <param name="encryptOpen">输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        void EncryptProtect(string filename, string password, string saveFileName = null, bool encryptOpen = true, bool readOnlyProtect = false);

        /// <summary>
        /// 导出图片png格式
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <param name="outputDirectory">保存图片的相对路径目录</param>
        /// <param name="httpRootPath">http绝对路径</param>
        /// <param name="password">文件打开密码</param>
        /// <returns></returns>
        string ImagePreview(string filename, string outputDirectory, string httpRootPath = "", string password = "");

        /// <summary>
        /// 导出文档pdf格式
        /// </summary>
        /// <param name="filename">word文件路径</param>
        /// <param name="outputDirectory">保存的相对路径目录</param>
        /// <param name="httpRootPath">http绝对路径</param>
        /// <param name="password">文件打开密码</param>
        /// <returns></returns>
        string PdfPreview(string filename, string outputDirectory, string httpRootPath = "", string password = "");

        /// <summary>
        /// 导出网页html格式
        /// </summary>
        /// <param name="filename">word文件路径</param>
        /// <param name="outputDirectory">保存的相对路径目录</param>
        /// <param name="httpRootPath">http绝对路径</param>
        /// <param name="password">文件打开密码</param>
        /// <returns></returns>
        string HtmlPreview(string filename, string outputDirectory, string httpRootPath = "", string password = "");

        /// <summary>
        /// 另存 Word 文档(*.pdf,*.html,*.png)
        /// </summary>
        /// <param name="source">来源文件(*.doc,*.docx)</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="outputFileFormat">选择一种输出格式(pdf,html,png)</param>
        /// <param name="uriString">返回文件路径前缀网址</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="name">返回文件名(不包含文件扩展类型)</param>
        /// <param name="ts">返回文件名后缀</param>
        /// <returns></returns>
        string SaveToFile(FileInfo source, string outputDirectory, string outputFileFormat, string uriString = null, string password = null, string name = null, string ts = null);
    }
}
