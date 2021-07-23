using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebFramework.Orm
{
    /// <summary>
    /// Values 存储 内存数据库
    /// </summary>
    public class ValuesDbContext : DbContext
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="options"></param>
        public ValuesDbContext(DbContextOptions<ValuesDbContext> options) : base(options) { }
        /// <summary>
        /// ValueModel DbSet
        /// </summary>
        public DbSet<ValueModel> StringValues { get; set; }
    }

    /// <summary>
    /// Values 存储 MSSQL
    /// </summary>
    public class ValuesContextOfSqlSugar : Orm.DbContextOfSqlSugar
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="options"></param>
        public ValuesContextOfSqlSugar(IOptions<ConnectionConfig> options) : base(options) { }

        /// <summary>
        /// ValueModel DbSet
        /// </summary>
        public Orm.DbTable<ValueModel> Students { get => new Orm.DbTable<ValueModel>(Client); }
    }

    /// <summary>
    /// Value Table Model
    /// </summary>
    [Table("Values"), SugarTable("Values")]
    public class ValueModel
    {
        /// <summary>
        /// Value ID 由数据库生成(非代码生成)
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// Value 最多长度50个字
        /// </summary>
        [Required, StringLength(50, ErrorMessage = "最多长度50个字")]
        public string Value { get; set; }
    }
}
