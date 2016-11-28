using System;
using System.ComponentModel.DataAnnotations;

namespace ED47.BusinessAccessLayer.Entities
{
    public class Email : BaseDbEntity
    {
        [MaxLength(250)]
        public virtual string BusinessKey { get; set; }

        [MaxLength(200)]
        public virtual string Recipient { get; set; }

        [MaxLength(200)]
        public virtual string FromAddress { get; set; }

        [MaxLength(500)]
        public virtual string Subject { get; set; }

        public virtual string Body { get; set; }

        [MaxLength(500)]
        public virtual string Signature { get; set; }

        public virtual DateTime?  TransmissionDate { get; set; }
    }
}
