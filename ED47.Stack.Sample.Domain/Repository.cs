using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ED47.BusinessAccessLayer;
using ED47.Stack.Sample.Entities;

namespace ED47.Stack.Sample.Domain
{
    public sealed class Repository : BusinessAccessLayer.Repository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Repository"/> class.
        /// </summary>
        /// <param name="context">The DB context.</param>
        /// <param name="immediateContext">The immediate DB context.</param>
        /// <param name="userName">The username.</param>
        public Repository(SampleContext context, SampleContext immediateContext, string userName = null)
            : base(context, immediateContext, userName)
        {
            ConnectionString = context.ConnectionString;
        }

        public Repository(string userName = null) :
            this(new SampleContext(), new SampleContext(), userName)
        {
        }

        internal new IEnumerable<TBusinessEntity> ExecuteStoredProcedure<TBusinessEntity>(string storedProcedure, params object[] parameters) where TBusinessEntity : class
        {
            return base.ExecuteStoredProcedure<TBusinessEntity>(storedProcedure, parameters);
        }

        internal new IEnumerable<TBusinessEntity> ExecuteStoredProcedure<TBusinessEntity>(string storedProcedure, params SqlParameter[] parameters) where TBusinessEntity : class
        {
            return base.ExecuteStoredProcedure<TBusinessEntity>(storedProcedure, parameters);
        }

        internal new void ApplyStoredProcedure<TEntity>(string storedProcedure, string idParameter, IEnumerable<TEntity> entities, Func<TEntity, IntKey> keySelector = null, Func<TEntity, object> entitySelector = null, Func<SqlDataReader, object> dataSelector = null, IEnumerable<SqlParameter> parameters = null)
        {
            base.ApplyStoredProcedure(storedProcedure, idParameter, entities, keySelector, entitySelector, dataSelector, parameters);
        }
    }
}
