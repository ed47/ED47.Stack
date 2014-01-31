using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;

namespace ED47.BusinessAccessLayer.Multilingual
{
    public interface IMultilingualRepository
    {
        IEnumerable<IMultilingual> GetPropertyTranslations(string businessKey, string propertyName);

        string T<TEntity, TBusinessEntity>(TBusinessEntity entity, Expression<Func<string>> propertySelector, string isoLanguageCode = null)
            where TBusinessEntity : IBusinessEntity
            where TEntity : DbEntity;

        string T<TBusinessEntity>(TBusinessEntity entity, Expression<Func<string>> propertySelector,
            string isoLanguageCode = null)
            where TBusinessEntity : IBusinessEntity;

        string T(string entityName, int entityId, Expression<Func<string>> propertySelector,
            string isoLanguageCode = null);

        void Commit();

        IEnumerable<IMultilingual> GetTranslations(IDictionary<string, object> keys, string isoLanguageCode = null, IEnumerable<string> properties = null, bool includeMissingTranslations = false);

        string GetTranslationtKey<TEntity, TBusinesEntity>(string entityName, TBusinesEntity item)
            where TEntity : DbEntity where TBusinesEntity : IBusinessEntity, new();

        IEnumerable<PropertyInfo> GetMultilingualProperties<TEntity>() where TEntity : class;
        void Upsert(IEnumerable<IMultilingual> multilinguals);

        void Translate<TEntity, TBusinesEntity>(IEnumerable<TBusinesEntity> businessEntities, string isoLanguageCode = null)
            where TEntity : DbEntity
            where TBusinesEntity : IBusinessEntity, new();

        void Translate<TEntity, TBusinesEntity>(TBusinesEntity businessEntity, string isoLanguageCode = null)
            where TEntity : DbEntity
            where TBusinesEntity : BusinessEntity, new();
    }
}
