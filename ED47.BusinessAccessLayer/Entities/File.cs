using System;
using System.ComponentModel.DataAnnotations;

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

        public virtual Boolean LoginRequired { get; set; }

        [MaxLength(100)]
        public virtual string MimeType { get; set; }
       
    }
}
