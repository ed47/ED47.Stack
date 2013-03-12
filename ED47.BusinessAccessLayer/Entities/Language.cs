using System.ComponentModel.DataAnnotations;

namespace ED47.BusinessAccessLayer.Entities
{
    /// <summary>
    /// Class containing all the language codes used in an application.
    /// </summary>
    public class Language :DbEntity
    {
        /// <summary>
        /// The 2-letter ISO code of the language.
        /// </summary>
        [Key]
        [MaxLength(2)]
        public virtual string IsoCode { get; set; }

        /// <summary>
        /// The languages name.
        /// </summary>
        [MultilingualProperty]
        [MaxLength(200)]
        public virtual string Name { get; set; }

    }
}
