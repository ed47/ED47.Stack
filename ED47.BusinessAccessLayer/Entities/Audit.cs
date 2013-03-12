using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ED47.BusinessAccessLayer.Entities
{
    public class Audit : DbEntity
    {
        [Key]
        public virtual int Id { get; set; }

        public virtual DateTime CreationDate { get; set; }

        [MaxLength(50)]
        public virtual string UserName { get; set; }
        
        public virtual string JsonData { get; set; }

        [MaxLength(100)]
        public virtual string SenderName { get; set; }

        [MaxLength(100)]
        public virtual string ActionName { get; set; }

        public virtual ICollection<AuditFilter> AuditFilters { get; set; }
    }
}
