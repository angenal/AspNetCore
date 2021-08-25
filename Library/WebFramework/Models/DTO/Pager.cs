using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebFramework.Models.DTO
{
    /// <summary>
    /// 数据库分页查询
    /// </summary>
    public static class SqlSugarExtensions
    {
        /// <summary></summary>
        public static PageOutputDto<T> ToPage<T>(this ISugarQueryable<T> query, PagerInputDto input)
        {
            int pageIndex = Math.Max(1, input.PageIndex), pageSize = Math.Min(1000, Math.Max(1, input.PageSize)), totalNumber = 0;
            var data = query.ToOffsetPage(pageIndex, pageSize, ref totalNumber);
            var result = new PageOutputDto<T>(data, pageIndex, pageSize, totalNumber);
            return result;
        }
        /// <summary></summary>
        public static async Task<PageOutputDto<T>> ToPageAsync<T>(this ISugarQueryable<T> query, PagerInputDto input)
        {
            RefAsync<int> totalNumber = 0;
            int pageIndex = Math.Max(1, input.PageIndex), pageSize = Math.Min(1000, Math.Max(1, input.PageSize));
            var data = await query.ToOffsetPageAsync(pageIndex, pageSize, totalNumber);
            var result = new PageOutputDto<T>(data, pageIndex, pageSize, totalNumber.Value);
            return result;
        }
    }

    /// <summary>
    /// 分页输入
    /// </summary>
    public class PagerInputDto
    {
        /// <summary>
        /// 第几页
        /// </summary>
        public int PageIndex { get; set; } = 1;
        /// <summary>
        /// 分页大小
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
    /// <summary>
    /// 分页输出
    /// </summary>
    public class PageOutputDto<T>
    {
        /// <summary>
        ///
        /// </summary>
        public PageOutputDto(int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
        /// <summary>
        ///
        /// </summary>
        public PageOutputDto(IEnumerable<T> data, int pageIndex, int pageSize, int pageNumber)
        {
            Data = data;
            PageIndex = pageIndex;
            PageSize = pageSize;
            PageNumber = pageNumber;
        }
        /// <summary>
        /// 分页数据
        /// </summary>
        public IEnumerable<T> Data { get; set; }
        /// <summary>
        /// 第几页
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 分页大小
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 分页数量
        /// </summary>
        public int PageNumber { get; set; }
    }

}
