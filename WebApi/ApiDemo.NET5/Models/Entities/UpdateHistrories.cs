using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ApiDemo.NET5.Models.Entities
{
    /// <summary>
    /// 历史记录
    /// </summary>
    public static class UpdateHistrories
    {
        /// <summary>
        /// 创建数据修改的历史记录
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="oldModel"></param>
        /// <param name="newInput"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static List<UpdateHistoryRecord> GetRecords<T1, T2>(T1 oldModel, T2 newInput, params string[] excludes)
        {
            var records = new List<UpdateHistoryRecord>();
            Type type1 = typeof(T1), type2 = typeof(T2);
            foreach (var p in type1.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                if (excludes.Any(u => u.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var attribute = Attribute.GetCustomAttribute(p, typeof(DisplayAttribute)) as DisplayAttribute;
                if (attribute == null)
                    continue;

                var record = new UpdateHistoryRecord
                {
                    FieldName = attribute.Name?.Replace("＆", " "),
                    OldValue = p.GetValue(oldModel)?.ToString(),
                    NewValue = type2.GetProperty(p.Name)?.GetValue(newInput)?.ToString()
                };
                if (record.OldValue == record.NewValue || Convert.ToString(record.OldValue ?? "").Trim() == Convert.ToString(record.NewValue ?? "").Trim())
                    continue;

                records.Add(record);
            }
            return records;
        }
    }

    /// <summary>
    /// 历史记录
    /// </summary>
    public partial class UpdateHistoryRecord
    {
        /// <summary>
        /// 编号
        /// </summary>
        [Display(Name = "编号")]
        [Key()]
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// 关联表
        /// </summary>
        [Display(Name = "关联表")]
        [MaxLength(20)]
        [Required]
        public string Tid { get; set; }

        /// <summary>
        /// 关联表编号
        /// </summary>
        [Display(Name = "关联表编号")]
        [Required]
        public int Pid { get; set; }

        /// <summary>
        /// 修改项
        /// </summary>
        [Display(Name = "修改项")]
        [MaxLength(400)]
        [Required]
        public string FieldName { get; set; }

        /// <summary>
        /// 修改前
        /// </summary>
        [Display(Name = "修改前")]
        [MaxLength(400)]
        public string OldValue { get; set; }

        /// <summary>
        /// 修改后
        /// </summary>
        [Display(Name = "修改后")]
        [MaxLength(400)]
        public string NewValue { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Display(Name = "修改人")]
        [MaxLength(100)]
        public string ModifierUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Display(Name = "修改时间")]
        [Required]
        public DateTime ModificationTime { get; set; }
    }
}
