using System.ComponentModel.DataAnnotations;

namespace ED47.BusinessAccessLayer.Entities
{
    public class FileBox : BaseDbEntity
    {
        [MaxLength(250)]
        public virtual string ParentTypeName { get; set; }
    }
}