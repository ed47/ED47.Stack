using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace ED47.BusinessAccessLayer.Entities
{
    public class Discussion : DbEntity
    {
        [Key]
        [MaxLength(250)]
        public virtual string BusinessKey { get; set; }
        public virtual bool IsEncrypted { get; set; }
        public virtual int? CommentOrder { get; set; }
        public virtual int? MaxComments { get; set; }
        public virtual bool IsReadOnly { get; set; }
    }
}
