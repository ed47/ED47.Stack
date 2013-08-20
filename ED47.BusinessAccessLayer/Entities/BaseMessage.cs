using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ED47.BusinessAccessLayer.Entities
{
    public class BaseMessage : BaseDbEntity
    {
        [MaxLength(60)]
        public virtual string MessageType { get; set; }
        
        [MaxLength(200)]
        public virtual string BusinessKey { get; set; }
       
        public virtual int? EmailId { get; set; }
        [ForeignKey("EmailId")]
        public virtual Email Email { get; set; }

        public virtual DateTime? OpenDate { get; set; }

        [MaxLength(500)]
        public virtual string GroupLabel { get; set; }

        [MaxLength(5)]
        public virtual string LanguageCode { get; set; }

    }
}