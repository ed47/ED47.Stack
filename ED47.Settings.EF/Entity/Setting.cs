using System.ComponentModel.DataAnnotations;
using ED47.BusinessAccessLayer;

namespace ED47.Settings.EF.Entity
{
    public class Setting : DbEntity, ED47.Settings.Entity.ISetting
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(500)]
        public string Path { get; set; }

        [MaxLength(500)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(1000)]
        public string Value { get; set; }
        
       // public bool IsPublic { get; set; }
    }
}
