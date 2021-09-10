using System.ComponentModel;

namespace WebInterface
{
    /// <summary>
    /// 语言代码 Locale: http://www.lingoes.cn/zh/translator/langcode.htm
    /// </summary>
    public enum Language
    {
        /// <summary>
        /// 英文
        /// </summary>
        [Description("en-US")]
        English,
        /// <summary>
        /// 中文
        /// </summary>
        [Description("zh-CN")]
        Chinese,
    }
}
