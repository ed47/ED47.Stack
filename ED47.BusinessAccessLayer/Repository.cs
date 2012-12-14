using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Transactions;

using Omu.ValueInjecter;

namespace ED47.BusinessAccessLayer
{
    public class Repository : IDisposable
    {
       

        /// <summary>
        /// The current Entity Framework DbContext.
        /// </summary>
        internal DbContext DbContext { get; set; }

        /// <summary>
        /// Gets or sets the immediate EF DbContext used for immediate operations (i.e. saving an entity without having to save all changes in the main context).
        /// </summary>
        /// <value>
        /// The immediate context.
        /// </value>
        protected DbContext ImmediateDbContext { get; set; }

        /// <summary>
        /// This repository's instance transaction.
        /// </summary>
        protected TransactionScope TransactionScope { get; set; }

        /// <summary>
        /// The username of the user who's request is attached to this repository.
        /// </summary>
        protected string UserName { get; set; }

        public string ConnectionString { get; set; }

        /// <summary>
        /// Creates a new Business Repository using a specific DbContext.
        /// </summary>
        /// <param name="dbcontext">The Entity Framework DbContext for this repository.</param>
        /// <param name="immediateDbcontext">The immediate dbcontext.</param>
        /// <param name="userName">The username of the user who's request is attached to this repository.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public Repository(DbContext dbcontext, DbContext immediateDbcontext, string userName = "[anonymous]")
        {
          
            DbContext = dbcontext;
            ImmediateDbContext = immediateDbcontext;
            
#if !DEBUG
            //this.TransactionScope = new TransactionScope();
#else
            //Infinite timeout to make debugging easier.
            //this.TransactionScope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(5));
#endif
            
            UserName = userName;
        }

        /// <summary>
        /// Commits the changes made on the context to the database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the database
        /// </returns>
        public int Commit()
        {
            var modifiedEntities = DbContext.ChangeTracker.Entries().Where(el => el.State == EntityState.Modified).ToList();
            var deletedEntities = DbContext.ChangeTracker.Entries().Where(el => el.State == EntityState.Deleted).ToList();
            
            var writtenObjectCount = DbContext.SaveChanges();

            foreach(var e in modifiedEntities.OfType<DbEntity>().Where(el=>el.BusinessEntity != null))
            {
                e.BusinessEntity.Commit();
                EventProxy.NotifyUpdated(e.BusinessEntity);
            }

            foreach (var e in deletedEntities.OfType<DbEntity>().Where(el => el.BusinessEntity != null))
            {
                e.BusinessEntity.Commit();
                EventProxy.NotifyDeleted(e.BusinessEntity);
            }

            var saveCounter = 0;
            while (DbContext.ChangeTracker.Entries().Any(el => el.State != EntityState.Unchanged))
            {
                if(saveCounter++ > 1000)
                    throw new RepositoryException("Repository tried saving more than set limit and there are still changes. Check for infinite recursion events or increase the limit.");

                writtenObjectCount += Commit();
            }

            return writtenObjectCount;
        }
        
        /// <summary>
        /// Finds an entity by its keys.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to find.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type.</typeparam>
        /// <param name="keys">The entities keys.</param>
        /// <returns></returns>
        [Obsolete("Use Find(predicated) instead as it adds auto-includes")]
        public TBusinessEntity Find<TEntity, TBusinessEntity>(params object[] keys)
            where TEntity : DbEntity
            where TBusinessEntity : class, new()
        {
            var result = DbContext.Set<TEntity>().Find(keys); 
            return result != null ? Convert<TEntity, TBusinessEntity>(result) : null;
        }

        /// <summary>
        /// Fins an entity with a predicate.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to find.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type to get.</typeparam>
        /// <param name="predicate">The filter predicate that must return a single result.</param>
        /// <param name="includes">The optional includes.</param>
        /// <returns></returns>
        public TBusinessEntity Find<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate, IEnumerable<string> includes = null)
            where TEntity :  DbEntity
            where TBusinessEntity : class, new()
        {
            return Where<TEntity, TBusinessEntity>(predicate, includes).SingleOrDefault();
        }


        /// <summary>
        /// Searches for entities using a predicate.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to get results in.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. </typeparam>
        /// <param name="predicate">The filtering predicate.</param>
        /// <param name="includes">The optional includes for this query.</param>
        /// <returns></returns>
        public IEnumerable<TBusinessEntity> Where<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate, IEnumerable<string> includes = null) 
            where TEntity : DbEntity
            where TBusinessEntity : class, new()
        {
            var query = DbContext.Set<TEntity>().AsQueryable();

            var businessPredicate = GetBusinessWherePredicate<TEntity, TBusinessEntity>();

            if (businessPredicate != null)
                query = query.Where(businessPredicate);

            query = AddIncludes(query, GetBusinessIncludes<TBusinessEntity>());
            query = AddIncludes(query, includes);

            return Convert<TEntity, TBusinessEntity>(query.Where(predicate));
        }

        /// <summary>
        /// Returns true if there are any elements.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. Its business predicate will also be applied if available,</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns></returns>
        public bool Any<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : DbEntity
            where TBusinessEntity : class, new()
        {
            var query = DbContext.Set<TEntity>().AsQueryable();

            var businessPredicate = GetBusinessWherePredicate<TEntity, TBusinessEntity>();
            
            if (businessPredicate != null)
                query = query.Where(businessPredicate);

            return query.Any(predicate);
        }

        /// <summary>
        /// Returns true if there all the elements match the condition.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. Its business predicate will also be applied if available,</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns></returns>
        public bool All<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : DbEntity
            where TBusinessEntity : class, new()
        {
            var query = DbContext.Set<TEntity>().AsQueryable();

            var businessPredicate = GetBusinessWherePredicate<TEntity, TBusinessEntity>();

            if (businessPredicate != null)
                query = query.Where(businessPredicate);

            return query.All(predicate);
        }

        /// <summary>
        /// Counts the items in a query.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. Its business predicate will also be applied if available,</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns></returns>
        public int Count<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : DbEntity
            where TBusinessEntity : class, new()
        {
            var query = DbContext.Set<TEntity>().AsQueryable();

            var businessPredicate = GetBusinessWherePredicate<TEntity, TBusinessEntity>();

            if (businessPredicate != null)
                query = query.Where(businessPredicate);

            return query.Count(predicate);
        }

        /// <summary>
        /// Sums the items in a query by the selector.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. Its business predicate will also be applied if available,</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <param name="selector">The selector function to select the field to sum.</param>
        /// <returns></returns>
        public decimal? Count<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, decimal?>> selector)
            where TEntity : DbEntity
            where TBusinessEntity : class, new()
        {
            var query = DbContext.Set<TEntity>().AsQueryable();

            var businessPredicate = GetBusinessWherePredicate<TEntity, TBusinessEntity>();

            if (businessPredicate != null)
                query = query.Where(businessPredicate);

            return query.Where(predicate).Sum(selector);
        }

        /// <summary>
        /// Sums the items in a query by the selector.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type. Its business predicate will also be applied if available,</typeparam>
        /// <param name="predicate">The filter predicate.</param>
        /// <param name="selector">The selector function to select the field to sum.</param>
        /// <returns></returns>
        public int? Count<TEntity, TBusinessEntity>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, int?>> selector)
            where TEntity : DbEntity
            where TBusinessEntity : class, new()
        {
            var query = DbContext.Set<TEntity>().AsQueryable();

            var businessPredicate = GetBusinessWherePredicate<TEntity, TBusinessEntity>();

            if (businessPredicate != null)
                query = query.Where(businessPredicate);

            return query.Where(predicate).Sum(selector);
        }

        /// <summary>
        /// Returns a queryable set.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The Business Entity type.</typeparam>
        /// <returns>A queryable set prefiltered by the business predicate.</returns>
        public IQueryable<TEntity> GetQueryableSet<TEntity, TBusinessEntity>() 
            where TEntity :DbEntity
            where TBusinessEntity : class, new()
        {
            var query = DbContext.Set<TEntity>().AsQueryable();
            query = AddIncludes(query, GetBusinessIncludes<TBusinessEntity>());

            var businessPredicate = GetBusinessWherePredicate<TEntity, TBusinessEntity>();
            if (businessPredicate != null)
                query = query.Where(businessPredicate);

            return query;
        }

        /// <summary>
        /// Gets all the elements of the defined type (business entity WherePredicate still applies).
        /// </summary>
        /// <typeparam name="TEntity">The Type of the Entity.</typeparam>
        /// <typeparam name="TBusinessEntity">The Type of the Business Entity.</typeparam>
        /// <param name="includes">The optional includes.</param>
        /// <returns></returns>
        public IEnumerable<TBusinessEntity> GetAll<TEntity, TBusinessEntity>(IEnumerable<string> includes = null)
            where TEntity : DbEntity
            where TBusinessEntity : class, new()
        {
            var businessPredicate = GetBusinessWherePredicate<TEntity, TBusinessEntity>();

            var query = businessPredicate == null ? DbContext.Set<TEntity>() : DbContext.Set<TEntity>().Where(businessPredicate);

            query = query.AsNoTracking();
            query = AddIncludes(query, includes);
            query = AddIncludes(query, GetBusinessIncludes<TBusinessEntity>());
            return Convert<TEntity, TBusinessEntity>(query);
        }

        /// <summary>
        /// Adds the includes to a query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="includes">The includes.</param>
        /// <returns></returns>
        protected static IQueryable<TEntity> AddIncludes<TEntity>(IQueryable<TEntity> query, IEnumerable<string> includes = null) where TEntity : DbEntity
        {
            return includes == null ? query : includes.Aggregate(query, (current, include) => current.Include(include));
        }

        /// <summary>
        /// Adds a new business entity to the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to add.</typeparam>
        /// <typeparam name="TBusinessEntity">The Type of the business entity to add.</typeparam>
        /// <param name="businessEntity">The business entity to add to the repository.</param>
        public void Add<TEntity, TBusinessEntity>(TBusinessEntity businessEntity)
            where TEntity : DbEntity, new()
            where TBusinessEntity :BusinessEntity
        {
            var newEntity = new TEntity();

            newEntity.InjectFrom(businessEntity);

            var baseDbEntity = newEntity as BaseDbEntity;
            if (baseDbEntity != null )
            {
                baseDbEntity.Guid = Guid.NewGuid();
                baseDbEntity.CreationDate = DateTime.UtcNow.ToUniversalTime();
                baseDbEntity.CreatorUsername = UserName;
                baseDbEntity.UpdateDate = baseDbEntity.CreationDate;
                baseDbEntity.UpdaterUsername = UserName;
                baseDbEntity.IsDeleted = false;
            }
            
            EventProxy.NotifyAdd(businessEntity);

            ImmediateDbContext.Set<TEntity>().Add(newEntity);
            ImmediateDbContext.SaveChanges();
            ImmediateDbContext.Entry(newEntity).State = EntityState.Detached;
            DbContext.Set<TEntity>().Attach(newEntity);

            //Get updated keys
            businessEntity.InjectFrom(newEntity);
            businessEntity.Init();

            EventProxy.NotifyAdded(businessEntity);
         
        }

        /// <summary>
        /// Adds a collection of new business entities to the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to add.</typeparam>
        /// <typeparam name="TBusinessEntity">The Type of the business entity to add.</typeparam>
        /// <param name="businessEntities">The collection of business entities to add to the repository.</param>
        public void Add<TEntity, TBusinessEntity>(IEnumerable<TBusinessEntity> businessEntities)
            where TEntity :  DbEntity, new()
            where TBusinessEntity : BusinessEntity
        {
            foreach (var businessEntity in businessEntities)
            {
                Add<TEntity, TBusinessEntity>(businessEntity);
            }
        }

        /// <summary>
        /// Updates a business entity to the repository.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to update.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type that's updating.</typeparam>
        /// <param name="businessEntity">The business entity that's updating.</param>
        /// <exception cref="RepositoryException">Fires this exception when the Entity is not found in the context.</exception>
        public void Update<TEntity, TBusinessEntity>(TBusinessEntity businessEntity)
            where TEntity :  DbEntity,new()
            where TBusinessEntity : BusinessEntity
        {
            IEnumerable<object> keys;
            var originalEntity = GetEntityFromBusinessEntity<TEntity, TBusinessEntity>(businessEntity, out keys);

            if (originalEntity == null)
            {
                var exception = new RepositoryException(Resources.Repository_Update_UpdateFailed_NotFound);
                exception.Data.Add("Entity Keys", keys);
                throw exception;
            }

            if (!EventProxy.NotifyUpdate(businessEntity))
                return;

           var dbEntity = originalEntity as BaseDbEntity;
            if (dbEntity != null)
            {
                dbEntity.UpdateDate = DateTime.UtcNow.ToUniversalTime();
                dbEntity.UpdaterUsername = UserName;
            }
           
            originalEntity.InjectFrom(businessEntity);
        }

        /// <summary>
        /// Updates a business entity collection to the repository.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to update.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type that's updating.</typeparam>
        /// <param name="businessEntities">The business entity collections that's updating.</param>
        /// <exception cref="RepositoryException">Fires this exception when the Entity is not found in the context.</exception>
        public void Update<TEntity, TBusinessEntity>(IEnumerable<TBusinessEntity> businessEntities)
            where TEntity :  DbEntity, new()
            where TBusinessEntity : BusinessEntity
        {
            foreach (var businessEntity in businessEntities)
            {
                Update<TEntity, TBusinessEntity>(businessEntity);    
            }
        }

        /// <summary>
        /// Gets an existing entity from a business entity.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type.</typeparam>
        /// <param name="businessEntity">The business entity.</param>
        /// <param name="keys">Returns the key values from the business entity.</param>
        /// <returns>The Entity instance that corresponds to the passed business entity.</returns>
        private TEntity GetEntityFromBusinessEntity<TEntity, TBusinessEntity>(TBusinessEntity businessEntity, out IEnumerable<object> keys)
            where TEntity :  DbEntity, new()
            where TBusinessEntity : BusinessEntity
        {
            var keyMembers = MetadataHelper.GetKeyMembers<TEntity>(DbContext);
            keys = businessEntity.GetKeyValues(keyMembers);
            var originalEntity = DbContext.Set<TEntity>().Find(keys.ToArray());
            return originalEntity;
        }

        public void Delete<TEntity, TBusinessEntity>(TBusinessEntity businessEntity)
            where TEntity :  DbEntity, new()
            where TBusinessEntity : BusinessEntity
        {

            IEnumerable<object> keys;
            var entity = GetEntityFromBusinessEntity<TEntity, TBusinessEntity>(businessEntity, out keys);

            if (!EventProxy.NotifyDelete(businessEntity))
                return;

            DbContext.Set<TEntity>().Remove(entity);
        }
        
        /// <summary>
        /// Converts an entity into a business entitity.
        /// </summary>
        /// <typeparam name="TSource">The Entity type.</typeparam>
        /// <typeparam name="TResult">The business entity type.</typeparam>
        /// <param name="source">The entity to convert.</param>
        /// <returns>A converted business entity.</returns>
        public static TResult Convert<TSource, TResult>(TSource source)
          
            where TResult : class, new()
        {
            if (source == null)
                return null;

            Type sourceType = typeof (TSource);
            Type targetType = typeof (TResult);
            TResult result = null;
            if (sourceType.IsSubclassOf(typeof(DbEntity))
                               && targetType.IsSubclassOf(typeof(BusinessEntity)))
            {
                var idprop = sourceType.GetProperty("Id");
                if (idprop != null && idprop.PropertyType == typeof(Int32))
                {
                    var id = System.Convert.ToInt32(idprop.GetValue(source, null));
                    result = BaseUserContext.TryGetDynamicInstance(targetType,id) as TResult;
                }
            }

            if (result == null)
            {
                result = new TResult();
                if (result is BusinessEntity)
                    BaseUserContext.StoreDynamicInstance(targetType,result as BusinessEntity );
            }
            result.InjectFrom<CustomFlatLoopValueInjection>(source);
            var businessEntity = result as BusinessEntity;
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

        /// <summary>
        /// Gets a Where predicate from the business entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TBusinessEntity">The business entity type.</typeparam>
        /// <returns>The predicate expression to filter by.</returns>
        private static Expression<Func<TEntity, bool>> GetBusinessWherePredicate<TEntity, TBusinessEntity>()
        {
            var type = typeof (TBusinessEntity);
            var whereClauseProprety = type
                                        .GetProperties(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                        .FirstOrDefault(el => el.Name == "Where");
            if(whereClauseProprety == null) return null;
            return whereClauseProprety.GetValue(null, null) as Expression<Func<TEntity, bool>>;
        }

        /// <summary>
        /// Gets the business type includes.
        /// </summary>
        /// <typeparam name="TBusinessEntity">The type of the business entity.</typeparam>
        /// <returns></returns>
        protected static IEnumerable<string> GetBusinessIncludes<TBusinessEntity>()
        {
            var type = typeof (TBusinessEntity);
            var includesMember = type.GetField("Includes",BindingFlags.FlattenHierarchy |  BindingFlags.Static | BindingFlags.NonPublic);
            if (includesMember == null) return null;
            return includesMember.GetValue(null) as IEnumerable<string>;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (TransactionScope != null)
                TransactionScope.Dispose();

            if(DbContext != null)
                DbContext.Dispose();

            if (ImmediateDbContext != null)
                ImmediateDbContext.Dispose();
        }


        protected internal IEnumerable<TBusinessEntity> ExecuteTableFunction<TBusinessEntity>(string tableFunction, params SqlParameter[] parameters) where TBusinessEntity : class
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var sqlcmd = conn.CreateCommand())
                {
                    InitSqlCommand(sqlcmd);

                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = "SELECT * FROM " + tableFunction + "(" +
                                         String.Join(",", parameters.Select(el => el.ParameterName)) + ")";

                    foreach (var parameter in parameters)
                    {
                        sqlcmd.Parameters.Add(parameter);
                    }

                    var reader = sqlcmd.ExecuteReader();

                    return ReaderInjection.ReadAll<TBusinessEntity>(reader).ToArray();
                }
            }

        }

        protected internal IEnumerable<TBusinessEntity> ExecuteTableFunction<TBusinessEntity>(string tableFunction, params object[] parameters) where TBusinessEntity : class
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var sqlcmd = conn.CreateCommand())
                {
                    InitSqlCommand(sqlcmd);
                    var sqlparams = new SqlParameter[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        sqlparams[i] = new SqlParameter("@p" + i, parameters[i]);
                    }

                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = "SELECT * FROM " + tableFunction + "(" +
                                         String.Join(",", sqlparams.Select(el => el.ParameterName)) + ")";
                    sqlcmd.Parameters.AddRange(sqlparams);

                  var reader = sqlcmd.ExecuteReader();

                    return ReaderInjection.ReadAll<TBusinessEntity>(reader).ToArray();
                }
            }

        }

        protected internal IEnumerable<TBusinessEntity> ExecuteStoredProcedure<TBusinessEntity>(string storedProcedure, params object[] parameters) where TBusinessEntity : class
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var sqlcmd = conn.CreateCommand())
                {
                    InitSqlCommand(sqlcmd);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.CommandText = storedProcedure;
                    var i = 0;
                    foreach (var parameter in parameters)
                    {
                        sqlcmd.Parameters.Add(new SqlParameter("@p" + i, parameter));
                        i++;
                    }

                    var reader = sqlcmd.ExecuteReader();

                    return ReaderInjection.ReadAll<TBusinessEntity>(reader).ToArray();
                }
            }
        }

        public IEnumerable<TBusinessEntity> ExecuteStoredProcedure<TBusinessEntity>(string storedProcedure, params SqlParameter[] parameters) where TBusinessEntity : class
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var sqlcmd = conn.CreateCommand())
                {
                    InitSqlCommand(sqlcmd);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.CommandText = storedProcedure;

                    foreach (var parameter in parameters)
                    {
                        sqlcmd.Parameters.Add(parameter);
                    }

                    using (var reader = sqlcmd.ExecuteReader())
                    {

                        return ReaderInjection.ReadAll<TBusinessEntity>(reader).ToArray();
                    }
                }
            }

        }

        private SqlDataReader ExecuteStoredProcedure(string storedProcedure, params SqlParameter[] parameters)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var sqlcmd = conn.CreateCommand())
                {
                    InitSqlCommand(sqlcmd);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.CommandText = storedProcedure;

                    foreach (var parameter in parameters)
                    {
                        sqlcmd.Parameters.Add(parameter);
                    }

                    return sqlcmd.ExecuteReader();
                }
            }

        }

        protected internal int ExecuteNonQuery(string storedProcedure, params SqlParameter[] parameters)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var sqlcmd = conn.CreateCommand())
                {
                    InitSqlCommand(sqlcmd);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.CommandText = storedProcedure;

                    foreach (var parameter in parameters)
                    {
                        sqlcmd.Parameters.Add(parameter);
                    }

                    return sqlcmd.ExecuteNonQuery();
                }
            }

        }




        /// <summary>
        /// Execute a stored procedure and apply the result on the collection
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="storedProcedure">The stored procedure.</param>
        /// <param name="idParameter">The id parameter.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="entitySelector">The entity selector.</param>
        /// <param name="dataSelector">The data selector.</param>
        public void ApplyStoredProcedure<TEntity>(string storedProcedure, string idParameter, IEnumerable<TEntity> entities, Func<TEntity, IntKey> keySelector = null, Func<TEntity, object> entitySelector = null, Func<SqlDataReader, object> dataSelector = null, IEnumerable<SqlParameter> parameters = null )
        {
            var idProp = typeof (TEntity).GetProperty("Id");
            var ks = keySelector ?? (el => new IntKey { Value = (System.Convert.ToInt32(idProp.GetValue(el, null))) });
            var ids = entities.Select(ks).Distinct().ToDataTable();
            var allParameters = new List<SqlParameter> {new SqlParameter(idParameter, ids)};

            if(parameters!=null)
            {
                allParameters.AddRange(parameters);
            }

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var sqlcmd = conn.CreateCommand())
                {
                    InitSqlCommand(sqlcmd);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.CommandText = storedProcedure;
                    sqlcmd.Parameters.AddRange(allParameters.ToArray());
                    using (var reader = sqlcmd.ExecuteReader())
                    {
                        entities.Apply(reader, entitySelector, dataSelector);

                    }
                }
            }
        }

        protected internal void ApplyStoredProcedure<TEntity>(string storedProcedure, string idParameter, IEnumerable<TEntity> entities, Func<TEntity, StringKey> keySelector = null, Func<TEntity, object> entitySelector = null, Func<SqlDataReader, object> dataSelector = null)
        {
            var idProp = typeof(TEntity).GetProperty("Id");
            var ks = keySelector ?? (el => new StringKey() { Value = idProp.GetValue(el, null).ToString() });
            var ids = entities.Select(ks).Distinct().ToDataTable();
            var p = new SqlParameter(idParameter, ids);

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var sqlcmd = conn.CreateCommand())
                {
                    InitSqlCommand(sqlcmd);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.CommandText = storedProcedure;
                    sqlcmd.Parameters.Add(p);
                    using (var reader = sqlcmd.ExecuteReader())
                    {
                        entities.Apply(reader, entitySelector, dataSelector);
                    }
                }
            }
        } 


        /// <summary>
        /// Softdeletes a entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keys">The keys.</param>
        public void SoftDelete<TEntity>(params object[] keys) where TEntity : BaseDbEntity
        {
            var element = DbContext.Set<TEntity>().Find(keys);
            if (element == null) return;

            element.DeletionDate = DateTime.UtcNow.ToUniversalTime();
            element.DeleterUsername = UserName;
            element.IsDeleted = true;
        }


        /// <summary>
        /// Initializes a Sql command with the webconfig appsettings "SQLCommandXxxxxxxx" elements
        /// </summary>
        /// <param name="sqlcmd">command wich will be setted the timeout </param>
        private void InitSqlCommand(SqlCommand sqlcmd)
        {
            // Set Timeout 
            int timeout = 0;
            var timeoutSetting = System.Configuration.ConfigurationManager.AppSettings["SqlCommandTimeout"];
            if (timeoutSetting != null)
                int.TryParse(timeoutSetting, out timeout);

            if (timeout > 0)
                sqlcmd.CommandTimeout = timeout;
        }
    }
}
