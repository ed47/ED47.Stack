using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
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

        // <summary>
        /// Gets the translations for a set of keys.
        /// </summary>
        /// <param name="isoLanguageCode">The ISO 2-letter language code.</param>
        /// <param name="dbContext">The EF DB Context.</param>
        /// <param name="key">The translation key to fetch.</param>
        /// <param name="propertyName">The optional name of the property to get translations for. If none is specified, all properties of the entity are retrieved.</param>
        /// <returns></returns>
        internal IEnumerable<IMultilingual> GetTranslations(string isoLanguageCode, string key, string propertyName = null)
        {
            if (String.IsNullOrWhiteSpace(key))
                return new List<BusinessEntities.Multilingual>(0);

            var objectSet = ((IObjectContextAdapter)BaseUserContext.Instance.Repository.DbContext).ObjectContext.CreateObjectSet<Entities.Multilingual>();
            var translations = objectSet.Where(m => m.LanguageIsoCode.ToLower() == isoLanguageCode && m.Key.Contains(key) && (propertyName == null || m.PropertyName == propertyName))
                .OrderBy(m => m.Key)
                .ThenBy(m => m.PropertyName);

            return RepositoryHelper.Convert<Entities.Multilingual, BusinessEntities.Multilingual>(translations).ToList();
        }

        public MvcHtmlString T<TEntity, TBusinessEntity>(TBusinessEntity entity, Expression<Func<string>> propertySelector, string isoLanguageCode = null)
            where TBusinessEntity : IBusinessEntity where TEntity : BusinessAccessLayer.DbEntity
        {
            if (isoLanguageCode == null)
                isoLanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            var bodyExpression = (MemberExpression)propertySelector.Body;
            var propertyName = bodyExpression.Member.Name;

            var key = typeof(TEntity).Name + "[" + String.Join(",", entity.GetKeys<TEntity>().Select(kv => kv.Value)) + "]";
            var translations = GetTranslations(isoLanguageCode, key, propertyName).ToList();
            var translation = translations.Any() ? translations.First().Text : propertySelector.Compile().Invoke();

            return new MvcHtmlString(translation);
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

        /// <summary>
        /// Gets the multilingual items from a collection of entity items using the values in their multilingual properties as the current translation.
        /// </summary>
        /// <typeparam name="TEntity">The entity types.</typeparam>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="dbContext">The EF DB Context</param>
        /// <param name="isoLanguageCode">The current ISO 2-letter language code.</param>
        public IEnumerable<IMultilingual> GetMultilinguals<TEntity>(IEnumerable<TEntity> entities, string isoLanguageCode) where TEntity : DbEntity
        {
            var properties = GetMultilingualProperties<TEntity>();

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var entity in entities)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                var key = entity.GetKey();

                foreach (var property in properties)
                {
                    var value = property.GetValue(entity, null);
                    var stringValue = value != null ? value.ToString() : null;

                    yield return new BusinessEntities.Multilingual
                    {
                        Key = key,
                        LanguageIsoCode = isoLanguageCode,
                        PropertyName = property.Name,
                        Text = stringValue
                    };
                }
            }
        }

        /// <summary>
        /// Gets the translations for a set of keys.
        /// </summary>
        /// <param name="isoLanguageCode">The ISO 2-letter language code.</param>
        /// <param name="dbContext">The EF DB Context.</param>
        /// <param name="keys">The translation keys to fetch.</param>
        /// <returns></returns>
        internal IEnumerable<IMultilingual> GetTranslations(string isoLanguageCode, IEnumerable<string> keys)
        {
            Debug.Assert(keys != null, "keys != null");
            var keyList = keys as string[] ?? keys.ToArray();
            if (!keyList.Any())
                return new List<BusinessEntities.Multilingual>(0);

            var set = ((IObjectContextAdapter)BaseUserContext.Instance.Repository.DbContext).ObjectContext.CreateObjectSet<Entities.Multilingual>();
            var ma = set.Where(m => m.LanguageIsoCode.ToLower() == isoLanguageCode.ToLower() && keyList.Contains(m.Key))
                .OrderBy(m => m.Key)
                .ThenBy(m => m.PropertyName);

            return Repository.Convert<Entities.Multilingual, BusinessEntities.Multilingual>(ma).ToList();
        }

        public MvcHtmlString T<TBusinessEntity>(TBusinessEntity entity, Expression<Func<string>> propertySelector, string isoLanguageCode = null)
            where TBusinessEntity : IBusinessEntity
        {
            if (isoLanguageCode == null)
                isoLanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            var bodyExpression = (MemberExpression)propertySelector.Body;
            var propertyName = bodyExpression.Member.Name;
            var entityType = typeof(TBusinessEntity);
            var key = entityType.Name + "[" + entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance).GetValue(entity, null) + "]";
            var translations = GetTranslations(isoLanguageCode, key, propertyName)
                .ToList();
            var translation = translations.Any() ? translations.First().Text : propertySelector.Compile().Invoke();

            return new MvcHtmlString(translation);
        }

        public MvcHtmlString T(string entityName, int entityId, Expression<Func<string>> propertySelector, string isoLanguageCode = null)
        {
            if (isoLanguageCode == null)
                isoLanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            var bodyExpression = (MemberExpression)propertySelector.Body;
            var propertyName = bodyExpression.Member.Name.Replace(entityName, String.Empty); //By convention, parent entity property name is "entityname + propertyname" (i.e.e RiskDescription)
            var key = entityName + "[" + entityId + "]";
            var translations = GetTranslations(isoLanguageCode, key, propertyName)
                .ToList();
            var translation = translations.Any() ? translations.First().Text : propertySelector.Compile().Invoke();

            return new MvcHtmlString(translation);
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
        public void Translate<TEntity, TBusinesEntity>(IEnumerable<TBusinesEntity> businessEntities, string isoLanguageCode)
            where TEntity : DbEntity
            where TBusinesEntity : IBusinessEntity, new()
        {
            businessEntities = businessEntities.Where(b => b != null).ToList();

            if (!businessEntities.Any())
                return;

            isoLanguageCode = isoLanguageCode.Trim().ToLower();
            var entityName = typeof(TBusinesEntity).Name;

            var keys = businessEntities.SelectMany(b => b.GetKeys<TEntity>().Select(kv => entityName + "[" + kv.Value + "]"));
            var translations = GetTranslations(isoLanguageCode, keys);

            foreach (var entity in businessEntities)
            {
                var item = entity; //HACK: To prevent modified closure bug in .Net 4.0

                var key = entityName + "[" + String.Join(",", item.GetKeys<TEntity>().Select(kv => kv.Value)) + "]";
                ApplyTranslation(item, translations.Where(t => t.Key.ToLower() == key.ToLower()));
            }
        }

        /// <summary>
        /// Translates a collection of entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <typeparam name="TBusinesEntity">The type of the entities.</typeparam>
        /// <param name="businessEntity">The collection of entities to translate.</param>
        /// <param name="isoLanguageCode">The 2-letter ISO code for the language to translate to.</param>
        /// <param name="repository">The Entity Framework DB Context.</param>
        public void Translate<TEntity, TBusinesEntity>(TBusinesEntity businessEntity, string isoLanguageCode)
            where TEntity : DbEntity
            where TBusinesEntity : BusinessEntity, new()
        {
            if (businessEntity == null)
                return;

            var entityName = typeof(TBusinesEntity).Name;
            var key = entityName + "[" + String.Join(",", businessEntity.GetKeys<TEntity>().Select(kv => kv.Value)) + "]";

            isoLanguageCode = isoLanguageCode.Trim().ToLower();
            var translations = GetTranslations(isoLanguageCode, key);
            ApplyTranslation(businessEntity, translations.Where(t => t.Key.Contains(key)));
        }
    }
}