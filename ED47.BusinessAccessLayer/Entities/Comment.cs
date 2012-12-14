using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.Entities
{
    public class Comment : BaseDbEntity
    {
        [MaxLength(250)]
        public virtual string BusinessKey { get; set; }
        public virtual string Body { get; set; }
        public virtual int? CommenterId { get; set; }
        public virtual int? FileBoxId { get; set; }
        [ForeignKey("FileBoxId")]
        public FileBox FileBox { get; set; }
    }
}
