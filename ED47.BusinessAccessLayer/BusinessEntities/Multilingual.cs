using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using ED47.BusinessAccessLayer.Multilingual;
using Ninject;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class Multilingual : BusinessEntity, IMultilingual
    {
        /// <summary>
        /// The composite key for the translation entry in the following format: EntityName[Id]
        /// For entities with multiple IDs, separate by commas with no spaces.
        /// </summary>
        public virtual string Key { get; set; }

        /// <summary>
        /// The translated property's name.
        /// </summary>
        public virtual string PropertyName { get; set; }

        /// <summary>
        /// The translation's ISO language code.
        /// </summary>
        [StringLength(2)]
        public virtual string LanguageIsoCode { get; set; }

        /// <summary>
        /// The translated text.
        /// </summary>
        [StringLength(500)]
        public virtual string Text { get; set; }

        

        public void Save()
        {
            var context = BaseUserContext.Instance;

            if (context == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            var translation = context.Repository.Find<Entities.Multilingual, Multilingual>(
                m =>
                m.Key == Key &&
                m.LanguageIsoCode == LanguageIsoCode &&
                m.PropertyName == PropertyName);
            if (translation == null)
            {
                context.Repository.Add<Entities.Multilingual, Multilingual>(this);
            }
            else
            {
                translation.Text = Text;
                context.Repository.Update<Entities.Multilingual, Multilingual>(translation);
            }
        }
        
        
    }
}
