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

        MvcHtmlString T<TEntity, TBusinessEntity>(TBusinessEntity entity, Expression<Func<string>> propertySelector, string isoLanguageCode = null)
            where TBusinessEntity : IBusinessEntity
            where TEntity : DbEntity;

        MvcHtmlString T<TBusinessEntity>(TBusinessEntity entity, Expression<Func<string>> propertySelector,
            string isoLanguageCode = null)
            where TBusinessEntity : IBusinessEntity;

        MvcHtmlString T(string entityName, int entityId, Expression<Func<string>> propertySelector,
            string isoLanguageCode = null);

        void Commit();

        IEnumerable<IMultilingual> GetTranslations(IDictionary<string, object> keys, string isoLanguageCode = null, IEnumerable<string> properties = null, bool includeMissingTranslations = false);

        string GeTranslationtKey<TEntity, TBusinesEntity>(string entityName, TBusinesEntity item)
            where TEntity : DbEntity where TBusinesEntity : IBusinessEntity, new();

        IEnumerable<PropertyInfo> GetMultilingualProperties<TEntity>() where TEntity : class;
    }
}
