using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.Entities
{
    public class Multilingual : DbEntity
    {
        /// <summary>
        /// The composite key for the translation entry in the following format: EntityName[Id]
        /// For entities with multiple IDs, separate by commas with no spaces.
        /// </summary>
        [Key]
        [Column(Order = 0)]
        public virtual string Key { get; set; }

        /// <summary>
        /// The translated property's name.
        /// </summary>
        [Key]
        [Column(Order = 1)]
        public virtual string PropertyName { get; set; }

        /// <summary>
        /// The translation's ISO language code.
        /// </summary>
        [Key]
        [Column(Order = 2)]
        [StringLength(2)]
        public virtual string LanguageIsoCode { get; set; }

        [ForeignKey("LanguageIsoCode")]
        public virtual Language Language { get; set; }

        /// <summary>
        /// The translated text.
        /// </summary>
        [StringLength(500)]
        public virtual string Text { get; set; }


    }
}
