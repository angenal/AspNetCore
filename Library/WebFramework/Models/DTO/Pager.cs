using SqlSugar;
using System;
using System.Threading.Tasks;
using WebCore.Models.DTO;

namespace WebFramework.Models.DTO
{
    /// <summary>
    /// 数据库分页查询 SqlSugar
    /// </summary>
    public static class SqlSugarPagerExtensions
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
}
