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
        void ExportWithDataTable(string saveFileName, DataTable dataTable, bool columnHeaders = true, int firstRow = 1, int firstColumn = 1, string password = null, bool readOnlyProtect = false);

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
        void ExportWithDataTable(Stream outputStream, DataTable dataTable, bool columnHeaders = true, int firstRow = 1, int firstColumn = 1, string password = null, bool readOnlyProtect = false);

        /// <summary>
        /// 另存 Excel 文档(*.pdf,*.html,*.png)
        /// </summary>
        /// <param name="source">来源文件(*.xls,*.xlsx)</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="outputFileFormat">选择一种输出格式(pdf,html,png)</param>
        /// <param name="uriString">返回文件路径前缀网址</param>
        /// <param name="password">文件密码,输入密码才能打开</param>
        /// <param name="name">返回文件名(不包含文件扩展类型)</param>
        /// <param name="ts">返回文件名后缀</param>
        /// <returns></returns>
        string SaveToFile(FileInfo source, string outputDirectory, string outputFileFormat, string uriString = null, string password = null, string name = null, string ts = null);

        /// <summary>
        /// Excel文件转换为DataSet
        /// </summary>
        /// <param name="path">Excel文件路径</param>
        /// <param name="columnsCount">表格列数限制</param>
        /// <param name="hasTitle">是否有表头</param>
        /// <param name="columnsName">表格列名列表</param>
        /// <param name="skipFirstColumnEmptyRow">跳过第一列为空的数据</param>
        /// <param name="getBackgroundColor">获取背景颜色RGB格式: #000000</param>
        /// <param name="trimText">清除单元格中的空白字符</param>
        DataSet ImportDataSet(string path, int[] columnsCount, bool[] hasTitle, string[][] columnsName, bool[] skipFirstColumnEmptyRow, bool[] getBackgroundColor, bool[] trimText);

        /// <summary>
        /// Excel文件流转换为DataSet
        /// </summary>
        /// <param name="stream">Excel文件流</param>
        /// <param name="worksheetIndex">工作表索引</param>
        /// <param name="columnsCount">表格列数限制</param>
        /// <param name="hasTitle">是否有表头</param>
        /// <param name="columnsName">表格列名列表</param>
        /// <param name="skipFirstColumnEmptyRow">跳过第一列为空的数据</param>
        /// <param name="getBackgroundColor">获取背景颜色RGB格式: #000000</param>
        /// <param name="trimText">清除单元格中的空白字符</param>
        DataSet ImportDataSet(Stream stream, int[] columnsCount, bool[] hasTitle, string[][] columnsName, bool[] skipFirstColumnEmptyRow, bool[] getBackgroundColor, bool[] trimText);

        /// <summary>
        /// Excel文件转换为DataTable
        /// </summary>
        /// <param name="path">Excel文件路径</param>
        /// <param name="worksheetIndex">工作表索引</param>
        /// <param name="columnsCount">表格列数限制</param>
        /// <param name="hasTitle">是否有表头</param>
        /// <param name="columnsName">表格列名列表</param>
        /// <param name="skipFirstColumnEmptyRow">跳过第一列为空的数据</param>
        /// <param name="getBackgroundColor">获取背景颜色RGB格式: #000000</param>
        /// <param name="trimText">清除单元格中的空白字符</param>
        DataTable ImportDataTable(string path, int worksheetIndex = 0, int columnsCount = 0, bool hasTitle = true, string[] columnsName = null, bool skipFirstColumnEmptyRow = true, bool getBackgroundColor = false, bool trimText = false);

        /// <summary>
        /// Excel文件流转换为DataTable
        /// </summary>
        /// <param name="stream">Excel文件流</param>
        /// <param name="worksheetIndex">工作表索引</param>
        /// <param name="columnsCount">表格列数限制</param>
        /// <param name="hasTitle">是否有表头</param>
        /// <param name="columnsName">表格列名列表</param>
        /// <param name="skipFirstColumnEmptyRow">跳过第一列为空的数据</param>
        /// <param name="getBackgroundColor">获取背景颜色RGB格式: #000000</param>
        /// <param name="trimText">清除单元格中的空白字符</param>
        DataTable ImportDataTable(Stream stream, int worksheetIndex = 0, int columnsCount = 0, bool hasTitle = true, string[] columnsName = null, bool skipFirstColumnEmptyRow = true, bool getBackgroundColor = false, bool trimText = false);
    }
}
