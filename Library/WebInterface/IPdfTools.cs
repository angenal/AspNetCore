using System.Collections.Generic;
using System.IO;
using WebInterface.Settings;

namespace WebInterface
{
    public interface IPdfTools
    {
        /// <summary>
        /// 另存 Pdf 文档(*.html,*.png)
        /// </summary>
        /// <param name="source">来源文件(*.pdf)</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="outputFileFormat">选择一种输出格式(html,png)</param>
        /// <param name="uriString">返回文件路径前缀网址</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="name">返回文件名(不包含文件扩展类型)</param>
        /// <param name="ts">返回文件名后缀</param>
        /// <returns></returns>
        string SaveToFile(FileInfo source, string outputDirectory, string outputFileFormat, string uriString = null, string password = null, string name = null, string ts = null);

        /// <summary>
        /// Apply digital signature
        /// </summary>
        /// <param name="unsignedDocument"></param>
        /// <param name="signedDocument"></param>
        /// <param name="hashAlgorithm"></param>
        /// <param name="signingReason"></param>
        /// <param name="signingLocation"></param>
        void Sign(string unsignedDocument, string signedDocument, string hashAlgorithm = "SHA256", string signingReason = null, string signingLocation = null);

        /// <summary>
        /// Gets digital signature infos
        /// </summary>
        /// <param name="signedDocument"></param>
        /// <returns></returns>
        IEnumerable<CertInfo> GetSignInfo(string signedDocument);
    }
}
