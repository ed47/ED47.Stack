using System.ComponentModel.DataAnnotations.Schema;

namespace ED47.BusinessAccessLayer
{
    public abstract class DbEntity
    {
        [NotMapped]
        internal IBusinessEntity BusinessEntity { get; set; }
    }
}
