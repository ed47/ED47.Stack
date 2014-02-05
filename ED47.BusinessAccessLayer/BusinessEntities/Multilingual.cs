using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Caching;
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
            var context = BaseUserContext.Instance ?? BusinessComponent.Kernel.Get<BaseUserContext>();

            var translations = context.Repository
                    .Where<Entities.Multilingual, Multilingual>(m =>
                        m.Key == Key &&
                        m.LanguageIsoCode == LanguageIsoCode &&
                        m.PropertyName == PropertyName)
                    .ToList();

            var translation = translations.SingleOrDefault(el => el.PropertyName == PropertyName);

            if (translation == null)
            {
                context.Repository.Add<Entities.Multilingual, Multilingual>(this);
                translations.Add(this);
            }
            else
            {
                translation.Text = Text;
                context.Repository.Update<Entities.Multilingual, Multilingual>(translation);
            }

            MemoryCache.Default.Set(new CacheItem(GetCacheKey(LanguageIsoCode, Key), translations), CacheItemPolicy);
        }

        public static string GetCacheKey(string language, string key)
        {
            return String.Concat("Translation", language, key);
        }

        public static CacheItemPolicy CacheItemPolicy
        {
            get
            {
                return new CacheItemPolicy
                {
#if !DEBUG
                    Priority = CacheItemPriority.NotRemovable
#endif
#if DEBUG
                    AbsoluteExpiration = DateTime.Now.AddSeconds(10)
#endif
                };
            }
        }
    }
}
