using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace WebCore.Models.DTO
{
    /// <summary>
    /// 数据库分页查询 LiteDb
    /// </summary>
    public static class LiteDbPagerEExtensions
    {
        /// <summary></summary>
        public static PageOutputDto<K> ToPage<T, K>(this ILiteQueryable<T> query, PagerInputDto input, Expression<Func<T, K>> selector)
        {
            int pageIndex = Math.Max(1, input.PageIndex), pageSize = Math.Min(1000, Math.Max(1, input.PageSize)), totalNumber = query.Count();
            IEnumerable<K> data = Array.Empty<K>();
            if (totalNumber > 0) data = query.Select<K>(selector).Skip(pageSize * (pageIndex - 1)).Limit(pageSize).ToList();
            var result = new PageOutputDto<K>(pageIndex, pageSize) { Data = data, PageNumber = (int)Math.Ceiling((double)totalNumber / input.PageSize) };
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
