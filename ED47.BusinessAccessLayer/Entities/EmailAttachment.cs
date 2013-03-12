using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.Entities
{
   public  class EmailAttachment : BaseDbEntity
    {


        public virtual int FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual File File { get; set; }


        public virtual int EmailId { get; set; }

        [ForeignKey("EmailId")]
        public virtual Email Email { get; set; }
    }
}
