using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using ED47.BusinessAccessLayer.BusinessEntities;
using ED47.BusinessAccessLayer.Multilingual;
using Ninject;

namespace ED47.BusinessAccessLayer.EF
{
    public class MultilingualRepository : IMultilingualRepository
    {
        public IEnumerable<IMultilingual> GetPropertyTranslations(string businessKey, string propertyName)
        {
            var context = BaseUserContext.Instance;

            if (context == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            var languages = Lang.GetLanguages();
            var translations = context.Repository.Where<Entities.Multilingual, BusinessEntities.Multilingual>(
                el => el.Key == businessKey && el.PropertyName == propertyName).ToList();

            var results = new List<IMultilingual>();
            foreach (var language in languages)
            {
                var translation = translations.SingleOrDefault(el => el.LanguageIsoCode == language.IsoCode);

                if (translation != null)
                {
                    results.Add(translation);
                }
                else
                {
                    results.Add(new BusinessEntities.Multilingual { Key = businessKey, PropertyName = propertyName, LanguageIsoCode = language.IsoCode });
                }
            }

            return results;
        }

        /// <summary>
        /// Gets the translations for a set of keys.
        /// </summary>
        /// <param name="isoLanguageCode">The ISO 2-letter language code.</param>
        /// <param name="key">The translation key to fetch.</param>
        /// <param name="propertyName">The optional name of the property to get translations for. If none is specified, all properties of the entity are retrieved.</param>
        /// <returns></returns>
        internal IEnumerable<IMultilingual> GetTranslations(string isoLanguageCode, string key, string propertyName = null)
        {
            if (String.IsNullOrWhiteSpace(key))
                return new List<BusinessEntities.Multilingual>(0);

            var cacheKey = BusinessEntities.Multilingual.GetCacheKey(isoLanguageCode, key);
            var translations = MemoryCache.Default.Get(cacheKey) as IEnumerable<IMultilingual>;

            if (translations == null)
            {
                var objectSet = ((IObjectContextAdapter)BaseUserContext.Instance.Repository.DbContext).ObjectContext.CreateObjectSet<Entities.Multilingual>();
                var translationQuery = objectSet.Where(m => m.LanguageIsoCode.ToLower() == isoLanguageCode && m.Key.Contains(key))
                                                .OrderBy(m => m.Key)
                                                .ThenBy(m => m.PropertyName);

                translations = RepositoryHelper.Convert<Entities.Multilingual, BusinessEntities.Multilingual>(translationQuery).ToList();

                MemoryCache.Default.Add(new CacheItem(cacheKey, translations), new CacheItemPolicy
                {
#if !DEBUG
                    Priority = CacheItemPriority.NotRemovable
#endif
#if DEBUG
                    AbsoluteExpiration = DateTime.Now.AddSeconds(10)
#endif
                });
            }

            return translations.Where(el => propertyName == null || el.PropertyName == propertyName);
        }

        public string T<TEntity, TBusinessEntity>(TBusinessEntity entity, Expression<Func<string>> propertySelector, string isoLanguageCode = null)
            where TBusinessEntity : IBusinessEntity
            where TEntity : BusinessAccessLayer.DbEntity
        {

            if (String.IsNullOrWhiteSpace(isoLanguageCode) && Thread.CurrentThread.CurrentUICulture.Name == "zh-CHS")
            {
                isoLanguageCode = "zs";
            }

            if (String.IsNullOrWhiteSpace(isoLanguageCode) && Thread.CurrentThread.CurrentUICulture.Name == "zh-CHT")
            {
                isoLanguageCode = "zt";
            }

            if (isoLanguageCode == null)
                isoLanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            var bodyExpression = (MemberExpression)propertySelector.Body;
            var propertyName = bodyExpression.Member.Name;

            var key = typeof(TEntity).Name + "[" + String.Join(",", entity.GetKeys<TEntity>().Select(kv => kv.Value)) + "]";
            var translations = GetTranslations(isoLanguageCode, key, propertyName).ToList();
            var translation = translations.Any() ? translations.First().Text : propertySelector.Compile().Invoke();

            return translation;
        }

        /// <summary>
        /// Gets the multilingual properties with values from an entity.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PropertyInfo> GetMultilingualProperties<TEntity>() where TEntity : class
        {
            //Get the real object type in case a proxy object is passed.
            var entityType = ObjectContext.GetObjectType(typeof(TEntity));
            var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            return properties.Where(p => p.GetCustomAttributes(inherit: false)
                                            .OfType<MultilingualPropertyAttribute>()
                                            .Any());
        }

        public void Upsert(IEnumerable<IMultilingual> multilinguals)
        {
            var enumerable = multilinguals as IMultilingual[] ?? multilinguals.ToArray();

            if (multilinguals == null || !enumerable.Any())
                return;

            foreach (var multilingual in enumerable)
            {
                if (String.IsNullOrWhiteSpace(multilingual.Text))
                    continue;

                multilingual.Save();
            }
        }

        public void Remove(IMultilingual multilingual)
        {
            if (multilingual == null)
                return;

            multilingual.Delete();
        }

        public IMultilingual Find(string key, string propertyName, string languageIsoCode)
        {
            return BaseUserContext.Instance.Repository
                    .Find<Entities.Multilingual, BusinessEntities.Multilingual>(el => el.Key == key && el.PropertyName == propertyName && el.LanguageIsoCode == languageIsoCode);
        }

        public IEnumerable<IMultilingual> GetTranslations(IDictionary<string, object> keys, string isoLanguageCode = null, IEnumerable<string> properties = null, bool includeMissingTranslations = false)
        {
            Debug.Assert(keys != null, "keys != null");

            if (!keys.Any())
                return new List<BusinessEntities.Multilingual>(0);

            var propertyNames = properties != null ? properties.ToList() : new List<string>();

            if (!propertyNames.Any() && includeMissingTranslations)
                Debug.Fail("You want missing translations but you haven't ");

            var set = ((IObjectContextAdapter)BaseUserContext.Instance.Repository.DbContext).ObjectContext.CreateObjectSet<Entities.Multilingual>();
            var ma = set.Where(m => (isoLanguageCode == null || m.LanguageIsoCode.ToLower() == isoLanguageCode.ToLower()) && keys.Keys.Contains(m.Key))
                .OrderBy(m => m.Key)
                .ThenBy(m => m.PropertyName);

            var multilinguals = Repository.Convert<Entities.Multilingual, BusinessEntities.Multilingual>(ma).ToList();

            var languages = Lang.GetLanguages().ToList();

            if (includeMissingTranslations)
            {
                foreach (var key in keys)
                {
                    foreach (var property in propertyNames)
                    {
                        var masterValue = key.Value
                                        .GetType()
                                        .GetProperty(property, BindingFlags.Public | BindingFlags.Instance)
                                        .GetValue(key.Value, null);

                        if (masterValue == null)
                            continue;

                        var master = masterValue.ToString();

                        multilinguals.Add(new BusinessEntities.Multilingual
                        {
                            Key = key.Key,
                            PropertyName = property,
                            LanguageIsoCode = "MASTER",
                            Text = master
                        });

                        foreach (var language in languages)
                        {
                            if (!multilinguals.Any(el =>
                                el.Key == key.Key
                                && el.PropertyName == property
                                && el.LanguageIsoCode.ToLowerInvariant() == language.IsoCode.ToLowerInvariant()))
                            {
                                string text = null;

                                if (language.IsoCode == ED47.Stack.Web.Properties.Settings.Default.DefaultLanguage.ToLowerInvariant())
                                    text = master;

                                multilinguals.Add(new BusinessEntities.Multilingual
                                {
                                    Key = key.Key,
                                    LanguageIsoCode = language.IsoCode,
                                    PropertyName = property,
                                    Text = text
                                });
                            }
                        }
                    }
                }
            }

            return multilinguals;
        }

        public IEnumerable<IMultilingual> GetTranslations(IEnumerable<string> keys, string isoLanguageCode = null)
        {
            Debug.Assert(keys != null, "keys != null");

            var enumerable = keys as string[] ?? keys.ToArray();
            if (!enumerable.Any())
                return new List<BusinessEntities.Multilingual>(0);

            var set = ((IObjectContextAdapter)BaseUserContext.Instance.Repository.DbContext).ObjectContext.CreateObjectSet<Entities.Multilingual>();
            var ma = set.Where(m => (isoLanguageCode == null || m.LanguageIsoCode.ToLower() == isoLanguageCode.ToLower()) && enumerable.Contains(m.Key))
                .OrderBy(m => m.Key)
                .ThenBy(m => m.PropertyName);

            return Repository.Convert<Entities.Multilingual, BusinessEntities.Multilingual>(ma).ToList();
        }

        public string T<TBusinessEntity>(TBusinessEntity entity, Expression<Func<string>> propertySelector, string isoLanguageCode = null)
            where TBusinessEntity : IBusinessEntity
        {
            if (entity == null)
                return String.Empty;


            if (String.IsNullOrWhiteSpace(isoLanguageCode) && Thread.CurrentThread.CurrentUICulture.Name == "zh-CHS")
            {
                isoLanguageCode = "zs";
            }

            if (String.IsNullOrWhiteSpace(isoLanguageCode) && Thread.CurrentThread.CurrentUICulture.Name == "zh-CHT")
            {
                isoLanguageCode = "zt";
            }


            if (isoLanguageCode == null)
                isoLanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            var bodyExpression = (MemberExpression)propertySelector.Body;
            var propertyName = bodyExpression.Member.Name;
            var entityType = typeof(TBusinessEntity);
            var key = entityType.Name + "[" + entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance).GetValue(entity, null) + "]";
            var translations = GetTranslations(isoLanguageCode, key, propertyName)
                .ToList();
            var translation = translations.Any() ? translations.First().Text : propertySelector.Compile().Invoke();

#if DEBUG
            //translation = "***" + translation;
#endif

            return translation;
        }

        public string T(string entityName, int entityId, Expression<Func<string>> propertySelector, string isoLanguageCode = null)
        {


            if (String.IsNullOrWhiteSpace(isoLanguageCode) && Thread.CurrentThread.CurrentUICulture.Name == "zh-CHS")
            {
                isoLanguageCode = "zs";
            }

            if (String.IsNullOrWhiteSpace(isoLanguageCode) && Thread.CurrentThread.CurrentUICulture.Name == "zh-CHT")
            {
                isoLanguageCode = "zt";
            }

            if (isoLanguageCode == null)
                isoLanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            var bodyExpression = (MemberExpression)propertySelector.Body;
            var propertyName = bodyExpression.Member.Name.Replace(entityName, String.Empty); //By convention, parent entity property name is "entityname + propertyname" (i.e.e RiskDescription)
            var key = entityName + "[" + entityId + "]";
            var translations = GetTranslations(isoLanguageCode, key, propertyName)
                .ToList();
            var translation = translations.Any() ? translations.First().Text : propertySelector.Compile().Invoke();

            return translation;
        }

        public void Commit()
        {
            var context = BaseUserContext.Instance;

            if (BaseUserContext.Instance == null) //This method can be called directly from the HTTP handler so it will use the default Context as defined in App_Start in that case
                context = BusinessComponent.Kernel.Get<BaseUserContext>();

            context.Commit();
        }

        /// <summary>
        /// Applies a translation to an instance of the object.
        /// </summary>
        /// <param name="entity">The entity instance to translate.</param>
        /// <param name="multilinguals">The multilingual items for this entity.</param>
        internal void ApplyTranslation(object entity, IEnumerable<IMultilingual> multilinguals)
        {
            var translatedEntityType = entity.GetType();

            foreach (var translation in multilinguals)
            {
                var property = translatedEntityType.GetProperty(translation.PropertyName);

                var text = String.IsNullOrEmpty(translation.Text) ? "### Missing translation ###" : translation.Text;

                if (property != null)
                    property.SetValue(entity, text, null);

            }
        }

        /// <summary>
        /// Translates a collection of entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <typeparam name="TBusinesEntity">The business entity type.</typeparam>
        /// <param name="businessEntities">The collection of entities to translate.</param>
        /// <param name="isoLanguageCode">The 2-letter ISO code for the language to translate to.</param>
        public void Translate<TEntity, TBusinesEntity>(IEnumerable<TBusinesEntity> businessEntities, string isoLanguageCode = null)
            where TEntity : DbEntity
            where TBusinesEntity : IBusinessEntity, new()
        {
            if (businessEntities == null)
                return;

            businessEntities = businessEntities.Where(b => b != null).ToList();

            if (!businessEntities.Any())
                return;


            if (String.IsNullOrWhiteSpace(isoLanguageCode) && Thread.CurrentThread.CurrentUICulture.Name == "zh-CHS")
            {
                isoLanguageCode = "zs";
            }

            if (String.IsNullOrWhiteSpace(isoLanguageCode) && Thread.CurrentThread.CurrentUICulture.Name == "zh-CHT")
            {
                isoLanguageCode = "zt";
            }

            if (String.IsNullOrWhiteSpace(isoLanguageCode))
                isoLanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            isoLanguageCode = isoLanguageCode.Trim().ToLowerInvariant();
            var entityName = typeof(TBusinesEntity).Name;

            var keys = businessEntities.SelectMany(b => b.GetKeys<TEntity>().Select(kv => entityName + "[" + kv.Value + "]"));
            var translations = GetTranslations(keys, isoLanguageCode).ToArray();

            foreach (var entity in businessEntities)
            {
                var item = entity; //HACK: To prevent modified closure bug in .Net 4.0

                var key = GetTranslationtKey<TEntity, TBusinesEntity>(entityName, item);
                ApplyTranslation(item, translations.Where(t => t.Key.ToLower() == key.ToLower()));
            }
        }

        public string GetTranslationtKey<TEntity, TBusinesEntity>(string entityName, TBusinesEntity item)
            where TEntity : DbEntity
            where TBusinesEntity : IBusinessEntity, new()
        {
            return entityName + "[" + String.Join(",", item.GetKeys<TEntity>().Select(kv => kv.Value)) + "]";
        }

        /// <summary>
        /// Translates a collection of entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <typeparam name="TBusinesEntity">The type of the entities.</typeparam>
        /// <param name="businessEntity">The collection of entities to translate.</param>
        /// <param name="isoLanguageCode">The 2-letter ISO code for the language to translate to.</param>
        public void Translate<TEntity, TBusinesEntity>(TBusinesEntity businessEntity, string isoLanguageCode = null)
            where TEntity : DbEntity
            where TBusinesEntity : BusinessEntity, new()
        {
            if (businessEntity == null)
                return;

            if (String.IsNullOrWhiteSpace(isoLanguageCode) && Thread.CurrentThread.CurrentUICulture.Name == "zh-CHS")
            {
                isoLanguageCode = "zs";
            }

            if (String.IsNullOrWhiteSpace(isoLanguageCode) && Thread.CurrentThread.CurrentUICulture.Name == "zh-CHT")
            {
                isoLanguageCode = "zt";
            }

            if (String.IsNullOrWhiteSpace(isoLanguageCode))
                isoLanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            var entityName = typeof(TBusinesEntity).Name;
            var key = entityName + "[" + String.Join(",", businessEntity.GetKeys<TEntity>().Select(kv => kv.Value)) + "]";

            isoLanguageCode = isoLanguageCode.Trim().ToLower();
            var translations = GetTranslations(isoLanguageCode, key);
            ApplyTranslation(businessEntity, translations.Where(t => t.Key.Contains(key)));
        }
    }
}