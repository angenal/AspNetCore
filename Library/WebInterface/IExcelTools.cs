using Spire.Xls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace WebInterface
{
    public interface IExcelTools
    {
        /// <summary>
        /// 导出 Excel 文档(*.xls,*.xlsx)
        /// </summary>
        /// <param name="templateFile">Excel模板文档</param>
        /// <param name="saveFileName">导出文件路径</param>
        /// <param name="list">数据列表</param>
        /// <param name="columnHeaders">列名关系{"A1","Hashtable:Key1"}</param>
        /// <param name="copyStyles">复制数据行第一行样式</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        void ExportWithList(string templateFile, string saveFileName, IEnumerable<Hashtable> list, Dictionary<string, string> columnHeaders, bool copyStyles = true, string password = null, bool readOnlyProtect = false);

        /// <summary>
        /// 导出 Excel 文档(*.xls,*.xlsx)
        /// </summary>
        /// <param name="templateFile">Excel模板文档</param>
        /// <param name="outputStream">输出字节流</param>
        /// <param name="list">数据列表</param>
        /// <param name="columnHeaders">列名关系{"A1","Hashtable:Key1"}</param>
        /// <param name="copyStyles">复制数据行第一行样式</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        void ExportWithList(string templateFile, Stream outputStream, IEnumerable<Hashtable> list, Dictionary<string, string> columnHeaders, bool copyStyles = true, string password = null, bool readOnlyProtect = false);

        /// <summary>
        /// 导出 Excel 文档(*.xls,*.xlsx)
        /// </summary>
        /// <param name="saveFileName">导出文件路径</param>
        /// <param name="dataTable">数据列表</param>
        /// <param name="columnHeaders">包含列名</param>
        /// <param name="firstRow">第一行</param>
        /// <param name="firstColumn">第一列</param>
        /// <param name="action">其它处理</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        void ExportWithDataTable(string saveFileName, DataTable dataTable, bool columnHeaders = true, int firstRow = 1, int firstColumn = 1, Action<Worksheet> action = null, string password = null, bool readOnlyProtect = false);

        /// <summary>
        /// 导出 Excel 文档(*.xls,*.xlsx)
        /// </summary>
        /// <param name="outputStream">输出字节流</param>
        /// <param name="dataTable">数据列表</param>
        /// <param name="columnHeaders">包含列名</param>
        /// <param name="firstRow">第一行</param>
        /// <param name="firstColumn">第一列</param>
        /// <param name="action">其它处理</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="readOnlyProtect">只读保护</param>
        void ExportWithDataTable(Stream outputStream, DataTable dataTable, bool columnHeaders = true, int firstRow = 1, int firstColumn = 1, Action<Worksheet> action = null, string password = null, bool readOnlyProtect = false);
    }
}
