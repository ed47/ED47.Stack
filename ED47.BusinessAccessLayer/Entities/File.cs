using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ED47.BusinessAccessLayer.Entities
{
    public class File : BaseDbEntity
    {
        public virtual int GroupId { get; set; }

        [MaxLength(200)]
        public virtual string BusinessKey { get; set; }

        [MaxLength(200)]
        public virtual string Name { get; set; }

        public virtual int Version { get; set; }

        [MaxLength(2)]
        public virtual string Lang { get; set; }
        [ForeignKey("Lang")]
        public virtual Language Language { get; set; }

        public virtual Boolean LoginRequired { get; set; }

        [MaxLength(100)]
        public virtual string MimeType { get; set; }

        public virtual bool Encrypted { get; set; }

        public string KeyHash { get; set; }
    }
}
