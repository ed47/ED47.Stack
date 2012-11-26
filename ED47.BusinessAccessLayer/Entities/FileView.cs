using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.Entities
{
    public class FileView : DbEntity
    {
        [Key]
        public virtual int Id { get; set; }

        public virtual DateTime? ViewDate { get; set; }

        [MaxLength(200)]
        public virtual string Username { get; set; }

        public virtual int FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual File File { get; set; }


    }
}
