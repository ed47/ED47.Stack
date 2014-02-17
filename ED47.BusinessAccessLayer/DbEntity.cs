using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ED47.BusinessAccessLayer
{
    public abstract class DbEntity
    {
        [NotMapped]
        [IgnoreDataMember]
        public IBusinessEntity BusinessEntity { get; set; }
    }
}
