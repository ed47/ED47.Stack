using System.ComponentModel.DataAnnotations;

namespace ED47.BusinessAccessLayer.Entities
{
    public class FileExtensionWhiteList : DbEntity
    {
        [Key]
        public virtual int Id { get; set; }

        [MaxLength(10)]
        public virtual string Extension { get; set; }
    }
}