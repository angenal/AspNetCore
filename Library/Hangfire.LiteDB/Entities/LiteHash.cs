using System;

namespace Hangfire.LiteDB.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class LiteHash : LiteExpiringKeyValue
    {

        /// <summary>
        /// 
        /// </summary>
        public string Field { get; set; }

    }
}