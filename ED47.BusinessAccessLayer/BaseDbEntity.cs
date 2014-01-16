using System;
using System.ComponentModel.DataAnnotations;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Base class for GrcTool Entities with soft-delete, a single pre-defined key and timestamp fields.
    /// For simpler or more custom classes inherit directly from DbEntity.
    /// </summary>
    public abstract class BaseDbEntity : DbEntity
    {
        [Key]
        public virtual int Id { get; set; }

        /// <summary>
        /// The creation date of the entity.
        /// </summary>
        public virtual DateTime CreationDate { get; set; }

        [MaxLength(200)]
        public virtual string CreatorUsername { get; set; }
        
        public virtual DateTime UpdateDate { get; set; }
        [MaxLength(200)]
        public virtual string UpdaterUsername { get; set; }

        public virtual bool IsDeleted { get; set; }
        public virtual DateTime? DeletionDate { get; set; }
        public virtual Boolean IsReadOnly { get; set; }
        [MaxLength(200)]
        public virtual string DeleterUsername { get; set; }

        public virtual Guid Guid { get; set; }

        /// <summary>
        /// Initialize all the properties inherited form BaseDbEntity
        /// if not yet done
        /// </summary>
        public TBusinessEntity Initialize<TBusinessEntity>(string username = "[system]") where TBusinessEntity : BaseDbEntity
        {
            if(Id > 0) return null;

            CreationDate = DateTime.UtcNow;
            CreatorUsername = username;
            UpdateDate = DateTime.UtcNow;
            UpdaterUsername = username;
            IsDeleted = false;
            IsReadOnly = false;
            Guid = Guid.NewGuid();

            return this as TBusinessEntity;

        }
    }
}
