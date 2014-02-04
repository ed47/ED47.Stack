using System;
using System.Linq.Expressions;

namespace ED47.BusinessAccessLayer.Multilingual
{
    public static class MultilingualRepository
    {
        public static string T<TBusinessEntity>(TBusinessEntity entity, Expression<Func<string>> propertySelector, string isoLanguageCode = null)
            where TBusinessEntity : IBusinessEntity
        {
            return MultilingualRepositoryFactory.Default.T(entity, propertySelector, isoLanguageCode);
        }
    }
}