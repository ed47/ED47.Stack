using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    }

    public static class MultilingualRepositoryExtensions
    {
        public static void SaveTranslations(this IMultilingualRepository repository, IEnumerable<Multilingual.IMultilingual> translations)
        {
            translations.ToList().ForEach(el => el.Save());
        }
    }
}
