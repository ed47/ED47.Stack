using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using Ninject;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class Multilingual : BusinessEntity
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


        ///// <summary>
        ///// Translates a collection of entities.
        ///// </summary>
        ///// <typeparam name="TEntity">The type of the entities.</typeparam>
        ///// <param name="entities">The collection of entities to translate.</param>
        ///// <param name="isoLanguageCode">The 2-letter ISO code for the language to translate to.</param>
        ///// <param name="dbContext">The Entity Framework DB Context.</param>
        //public static void Translate<TEntity>(IEnumerable<TEntity> entities, string isoLanguageCode) where TEntity : DbEntity
        //{
        //    if (!entities.Any())
        //        return;

        //    isoLanguageCode = isoLanguageCode.Trim().ToLower();
        //    var entitiesWithKeys = entities.ToDictionary(e => e.GetKey(dbContext));
        //    var translations = Multilingual.GetTranslations(isoLanguageCode, dbContext, entitiesWithKeys.Keys);

        //    foreach (var entityWithKey in entitiesWithKeys)
        //    {
        //        var item = entityWithKey; //HACK: To prevent modified closure bug in .Net 4.0
        //        Multilingual.ApplyTranslation(entityWithKey.Value, translations.Where(t => t.Key == item.Key));
        //    }
        //}

        /// <summary>
        /// Gets the translations for a set of keys.
        /// </summary>
        /// <param name="isoLanguageCode">The ISO 2-letter language code.</param>
        /// <param name="dbContext">The EF DB Context.</param>
        /// <param name="keys">The translation keys to fetch.</param>
        /// <returns></returns>
        internal static List<BusinessEntities.Multilingual> GetTranslations(string isoLanguageCode, IEnumerable<string> keys, IObjectContextAdapter dbContext) {
            Debug.Assert(keys != null, "keys != null");
            var keyList = keys as string[] ?? keys.ToArray();
            if (!keyList.Any())
                return new List<BusinessEntities.Multilingual>(0);

            var set = dbContext.ObjectContext.CreateObjectSet<Entities.Multilingual>();
            var ma = set.Where(m => m.LanguageIsoCode.ToLower() == isoLanguageCode.ToLower() && keyList.Contains(m.Key))
                .OrderBy(m => m.Key)
                .ThenBy(m => m.PropertyName);

            return Repository.Convert<Entities.Multilingual, BusinessEntities.Multilingual>(ma).ToList();

        }

        /// <summary>
        /// Gets the translations for a set of keys.
        /// </summary>
        /// <param name="isoLanguageCode">The ISO 2-letter language code.</param>
        /// <param name="dbContext">The EF DB Context.</param>
        /// <param name="key">The translation key to fetch.</param>
        /// <returns></returns>
        internal static IEnumerable<Multilingual> GetTranslations(string isoLanguageCode, string key, IObjectContextAdapter dbContext) {

            if (string.IsNullOrWhiteSpace(key))
                return new List<BusinessEntities.Multilingual>(0);

            var set = dbContext.ObjectContext.CreateObjectSet<Entities.Multilingual>();
            var ma = set.Where(m => m.LanguageIsoCode.ToLower() == isoLanguageCode && m.Key.Contains(key))
                .OrderBy(m => m.Key)
                .ThenBy(m => m.PropertyName);

            return Repository.Convert<Entities.Multilingual, BusinessEntities.Multilingual>(ma).ToList();

        }
       
       
        /// <summary>
        /// Applies a translation to an instance of the object.
        /// </summary>
        /// <param name="entity">The entity instance to translate.</param>
        /// <param name="multilinguals">The multilingual items for this entity.</param>
        internal static void ApplyTranslation(object entity, IEnumerable<Multilingual> multilinguals)
        {
            var translatedEntityType = entity.GetType();

            foreach (var translation in multilinguals)
            {
                var property = translatedEntityType.GetProperty(translation.PropertyName);

                var text = string.IsNullOrEmpty(translation.Text) ? "### Missing translation ###" : translation.Text;

                if (property != null)
                    property.SetValue(entity, text, null);

            }
        }

        /// <summary>
        /// Gets the multilingual items from a collection of entity items using the values in their multilingual properties as the current translation.
        /// </summary>
        /// <typeparam name="TEntity">The entity types.</typeparam>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="dbContext">The EF DB Context</param>
        /// <param name="isoLanguageCode">The current ISO 2-letter language code.</param>
        public static IEnumerable<Multilingual> GetMultilinguals<TEntity>(IEnumerable<TEntity> entities, IObjectContextAdapter dbContext, string isoLanguageCode) where TEntity : DbEntity
        {
            var properties = MetadataHelper.GetMultilingualProperties<TEntity>();

// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var entity in entities)
// ReSharper restore LoopCanBeConvertedToQuery
            {
                var key = entity.GetKey(dbContext);

                foreach (var property in properties)
                {
                    var value = property.GetValue(entity, null);
                    var stringValue = value != null ? value.ToString() : null;

                    yield return new Multilingual
                    {
                        Key = key,
                        LanguageIsoCode = isoLanguageCode,
                        PropertyName = property.Name,
                        Text = stringValue
                    };
                }
            }
        }

        public static IEnumerable<Multilingual> GetPropertyTranslations(string businessKey, string propertyName)
        {
            var context = BaseUserContext.Instance;

            if (context == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            var languages = Lang.GetLanguages();
            var translations = context.Repository.Where<Entities.Multilingual, Multilingual>(
                el => el.Key == businessKey && el.PropertyName == propertyName).ToList();

            var results = new List<Multilingual>();
            foreach (var language in languages)
            {
                var translation = translations.SingleOrDefault(el => el.LanguageIsoCode == language.IsoCode);

                if(translation != null)
                {
                    results.Add(translation);
                }
                else
                {
                    results.Add(new Multilingual { Key = businessKey, PropertyName = propertyName, LanguageIsoCode = language.IsoCode});
                }
            }
            
            return results;
        }

        public static void SaveTranslations(IEnumerable<Multilingual> translations)
        {
            translations.ToList().ForEach(el => el.Save());
        }

        private void Save()
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
            } else
            {
                translation.Text = Text;
                context.Repository.Update<Entities.Multilingual, Multilingual>(translation);
            }
        }
    }
}
