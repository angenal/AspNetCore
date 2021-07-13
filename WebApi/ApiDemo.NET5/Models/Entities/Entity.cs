using System;

namespace ApiDemo.NET5.Models.Entities
{
    /// <summary>
    /// Base Entity
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Unique Identifier
        /// </summary>
        public virtual Guid Id { get; set; }

        public virtual Guid CustomerId { get; set; } = Guid.Empty;

        public virtual int TenantId { get; set; }

        public virtual Guid? CreatorUserId { get; set; }
        public virtual DateTime CreationTime { get; set; }
        public virtual Guid? LastModifierUserId { get; set; }
        public virtual DateTime LastModificationTime { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual Guid? DeleterUserId { get; set; }
        public virtual DateTime? DeletionTime { get; set; }
    }
}
