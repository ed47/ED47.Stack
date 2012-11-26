using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ED47.BusinessAccessLayer
{
    public abstract class DbEntity
    {
        [NotMapped]
        internal BusinessEntity BusinessEntity { get; set; }
    }
}
