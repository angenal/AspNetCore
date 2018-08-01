using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using WebFramework.ORM;

namespace WebApiZ1.Models
{
    /// <summary>
    /// Values 存储 内存数据库
    /// </summary>
    public class ValuesContext : DbContext
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="options"></param>
        public ValuesContext(DbContextOptions<ValuesContext> options) : base(options) { }
        /// <summary>
        /// ValueModel Set
        /// </summary>
        public DbSet<ValueModel> StringValues { get; set; }
    }
    /// <summary>
    /// Values 存储 MSSQL
    /// </summary>
    public class ValuesContextOfSqlSugar : DbContextOfSqlSugar
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="options"></param>
        public ValuesContextOfSqlSugar(IOptions<ConnectionConfig> options) : base(options) { }
        /// <summary>
        /// ValueModel Set
        /// </summary>
        public DbSetOfSqlSugar<ValueModel> Students { get { return new DbSetOfSqlSugar<ValueModel>(Db); } }
    }
    /// <summary>
    /// Value Model
    /// </summary>
    [Table("Values"), SugarTable("Values")]
    public class ValueModel
    {
        /// <summary>
        /// Value ID 由代码生成
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        /// <summary>
        /// Value 最多长度50个字
        /// </summary>
        [Required, StringLength(50, ErrorMessage = "最多长度50个字")]
        public string Value { get; set; }
    }
}
