using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Omu.ValueInjecter;

namespace ED47.BusinessAccessLayer
{
    public interface IRepository : IDisposable
    {
        string ConnectionString { get; set; }

        //TODO: Remove this property once all usages are moved to BusinessAccess.EF
        DbContext DbContext { get; set; }

        /// <summary>
        /// Commits the changes made on the context to the database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the database
        /// </returns>
        int Commit();
        
        /// <summary>
        /// Fins an entity with a predicate.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to find.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type to get.</typeparam>
        /// <param name="predicate">The filter predicate that must return a single result.</param>
        /// <param name="includes">The optional includes.</param>
        /// <param name="ignoreBusinessPredicate"></param>
        /// <returns></returns>
        TBusinessEntity Find<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate, IEnumerable<string> includes = null, bool ignoreBusinessPredicate = false)
            where TEntity : DbEntity
            where TBusinessEntity : class, new();

        /// <summary>
        /// Searches for entities using a predicate.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to get results in.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. </typeparam>
        /// <param name="predicate">The filtering predicate.</param>
        /// <param name="includes">The optional includes for this query.</param>
        /// <returns></returns>
        IEnumerable<TBusinessEntity> Where<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate, IEnumerable<string> includes = null, bool ignoreBusinessPredicate = false)
            where TEntity : DbEntity
            where TBusinessEntity : class, new();

        /// <summary>
        /// Returns true if there are any elements.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. Its business predicate will also be applied if available,</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns></returns>
        bool Any<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : DbEntity
            where TBusinessEntity : class, new();

        /// <summary>
        /// Returns true if there all the elements match the condition.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. Its business predicate will also be applied if available,</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns></returns>
        bool All<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : DbEntity
            where TBusinessEntity : class, new();

        /// <summary>
        /// Counts the items in a query.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. Its business predicate will also be applied if available,</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns></returns>
        int Count<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : DbEntity
            where TBusinessEntity : class, new();

        /// <summary>
        /// Sums the items in a query by the selector.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. Its business predicate will also be applied if available,</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <param name="selector">The selector function to select the field to sum.</param>
        /// <returns></returns>
        decimal? Sum<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, decimal?>> selector)
            where TEntity : DbEntity
            where TBusinessEntity : class, new();

        /// <summary>
        /// Sums the items in a query by the selector.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. Its business predicate will also be applied if available,</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <param name="selector">The selector function to select the field to sum.</param>
        /// <returns></returns>
        int? Sum<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, int?>> selector)
            where TEntity : DbEntity
            where TBusinessEntity : class, new();

        /// <summary>
        /// Gets the maximum value for a entity property.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. Its business predicate will also be applied if available,</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <param name="selector">The selector function to select the field to get the maximum for.</param>
        /// <returns></returns>
        int? Max<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, int?>> selector)
            where TEntity : DbEntity
            where TBusinessEntity : class, new();

        /// <summary>
        /// Returns a queryable set.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The Business Entity type.</typeparam>
        /// <returns>A queryable set prefiltered by the business predicate.</returns>
        IQueryable<TEntity> GetQueryableSet<TEntity, TBusinessEntity>()
            where TEntity : DbEntity
            where TBusinessEntity : class, new();

        /// <summary>
        /// Gets all the elements of the defined type (business entity WherePredicate still applies).
        /// </summary>
        /// <typeparam name="TEntity">The Type of the Entity.</typeparam>
        /// <typeparam name="TBusinessEntity">The Type of the Business Entity.</typeparam>
        /// <param name="includes">The optional includes.</param>
        /// <returns></returns>
        IEnumerable<TBusinessEntity> GetAll<TEntity, TBusinessEntity>(IEnumerable<string> includes = null)
            where TEntity : DbEntity
            where TBusinessEntity : class, new();

        /// <summary>
        /// Adds a new business entity to the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to add.</typeparam>
        /// <typeparam name="TBusinessEntity">The Type of the business entity to add.</typeparam>
        /// <param name="businessEntity">The business entity to add to the repository.</param>
        void Add<TEntity, TBusinessEntity>(TBusinessEntity businessEntity)
            where TEntity : DbEntity, new()
            where TBusinessEntity : IBusinessEntity;

        /// <summary>
        /// Adds a collection of new business entities to the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to add.</typeparam>
        /// <typeparam name="TBusinessEntity">The Type of the business entity to add.</typeparam>
        /// <param name="businessEntities">The collection of business entities to add to the repository.</param>
        void Add<TEntity, TBusinessEntity>(IEnumerable<TBusinessEntity> businessEntities)
            where TEntity : DbEntity, new()
            where TBusinessEntity : IBusinessEntity;

        /// <summary>
        /// Updates a business entity to the repository.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to update.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type that's updating.</typeparam>
        /// <param name="businessEntity">The business entity that's updating.</param>
        /// <exception cref="RepositoryException">Fires this exception when the Entity is not found in the context.</exception>
        void Update<TEntity, TBusinessEntity>(TBusinessEntity businessEntity)
            where TEntity : DbEntity, new()
            where TBusinessEntity : IBusinessEntity;

        /// <summary>
        /// Updates a business entity collection to the repository.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to update.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type that's updating.</typeparam>
        /// <param name="businessEntities">The business entity collections that's updating.</param>
        /// <exception cref="RepositoryException">Fires this exception when the Entity is not found in the context.</exception>
        void Update<TEntity, TBusinessEntity>(IEnumerable<TBusinessEntity> businessEntities)
            where TEntity : DbEntity, new()
            where TBusinessEntity : IBusinessEntity;

        void Delete<TEntity, TBusinessEntity>(TBusinessEntity businessEntity)
            where TEntity : DbEntity, new()
            where TBusinessEntity : IBusinessEntity;

        /// <summary>
        /// Softdeletes a entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keys">The keys.</param>
        void SoftDelete<TEntity>(params object[] keys) where TEntity : BaseDbEntity;

        /// <summary>
        /// Restores a soft deleted entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="keys">The entity's keys.</param>
        TBusinessEntity Restore<TEntity, TBusinessEntity>(params object[] keys) 
            where TEntity : BaseDbEntity 
            where TBusinessEntity : class, new();
    }

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