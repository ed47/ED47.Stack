using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;

namespace ED47.BusinessAccessLayer.EF
{
    public interface ISqlRepository : IRepository
    {
        IEnumerable<TBusinessEntity> ExecuteTableFunction<TBusinessEntity>(string tableFunction, params object[] parameters) where TBusinessEntity : class;
        IEnumerable<TBusinessEntity> ExecuteStoredProcedure<TBusinessEntity>(string storedProcedure, params object[] parameters) where TBusinessEntity : class;
        IEnumerable<TBusinessEntity> ExecuteStoredProcedure<TBusinessEntity>(string storedProcedure, params SqlParameter[] parameters) where TBusinessEntity : class;

        /// <summary>
        /// Execute a stored procedure which returns N tables.
        /// First table will be mapped to objects of type types[0], second table with type types[1] and so on.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="types">must be length N</param>
        /// <param name="parameters"></param>
        /// <returns>length is N. Result[0] will be the objects (of type types[0]) from the first table, ...</returns>
        object[][] ExecuteStoredProcedure(string storedProcedure, Type[] types, params SqlParameter[] parameters);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "SQL query is parametrized")]
        int ExecuteNonQuery(string storedProcedure, params SqlParameter[] parameters);

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
        /// <param name="parameters">The parameters of the stored procedure.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "SQL query is parametrized.")]
        void ApplyStoredProcedure<TEntity>(string storedProcedure, string idParameter, IEnumerable<TEntity> entities, Func<TEntity, IntKey> keySelector = null, Func<TEntity, object> entitySelector = null, Func<DbDataReader, object> dataSelector = null, IEnumerable<SqlParameter> parameters = null);
    }
}