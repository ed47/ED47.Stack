using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class EmailAttachment : BusinessEntity
    {
#pragma warning disable 169
        private static readonly string[] Includes = new[] { "File"};
#pragma warning restore 169

        public virtual int Id { get; set; }

        public virtual int FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual File File { get; set; }


        public virtual int EmailId { get; set; }

        [ForeignKey("EmailId")]
        public virtual Email Email { get; set; }




        

    }
}
