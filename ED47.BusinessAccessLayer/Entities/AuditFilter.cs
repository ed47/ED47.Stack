using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ED47.BusinessAccessLayer.Entities
{
    public class AuditFilter : DbEntity
    {
        [Key]
        [Column(Order = 2)]
        public virtual int AuditId { get; set; }
        [ForeignKey("AuditId")]
        public virtual Audit Audit { get; set; }

        [Key]
        [Column(Order = 0)]
        [MaxLength(50)]
        public virtual string ReferenceName { get; set; }

        [Key]
        [Column(Order = 1)]
        public virtual int ReferenceValue { get; set; }
    }
}
