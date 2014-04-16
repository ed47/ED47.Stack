using System;
using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;

namespace ED47.BusinessAccessLayer
{
    public static class RepositoryHelper
    {
        /// <summary>
        /// Converts an entity into a business entitity.
        /// </summary>
        /// <typeparam name="TSource">The Entity type.</typeparam>
        /// <typeparam name="TResult">The business entity type.</typeparam>
        /// <param name="source">The entity to convert.</param>
        /// <returns>A converted business entity.</returns>
        public static TResult Convert<TSource, TResult>(TSource source) where TResult : class, new()
        {
            if (source == null)
                return null;

            var sourceType = typeof(TSource);
            var targetType = typeof(TResult);
            TResult result = null;

            if (sourceType.IsSubclassOf(typeof(DbEntity))
                && targetType.IsSubclassOf(typeof(IBusinessEntity)))
            {
                var idprop = sourceType.GetProperty("Id");
                if (idprop != null && idprop.PropertyType == typeof(Int32))
                {
                    var id = System.Convert.ToInt32(idprop.GetValue(source, null));
                    result = BaseUserContext.TryGetDynamicInstance(targetType, id) as TResult;
                }
            }

            if (result == null)
            {
                result = new TResult();
                if (result is IBusinessEntity)
                    BaseUserContext.StoreDynamicInstance(targetType, result as IBusinessEntity);
            }
            result.InjectFrom<CustomFlatLoopValueInjection>(source);
            result.ReadJsonData(source);
            Cryptography.DecryptProperties(result);
            var businessEntity = result as IBusinessEntity;
            var dbEntity = source as DbEntity;
            if (businessEntity != null && dbEntity != null)
            {
                dbEntity.BusinessEntity = businessEntity;
                businessEntity.Init();
            }
            return result;
        }

        /// <summary>
        /// Converts a collection of entities into business entitites.
        /// </summary>
        /// <typeparam name="TSource">The Entity type.</typeparam>
        /// <typeparam name="TResult">The business entity type.</typeparam>
        /// <param name="sources">The collection of entities to convert.</param>
        /// <returns>A collection of converted business entities.</returns>
        public static IEnumerable<TResult> Convert<TSource, TResult>(IEnumerable<TSource> sources)
            where TSource : DbEntity
            where TResult : class, new()
        {
            return sources.Select(Convert<TSource, TResult>);
        }
    }
}