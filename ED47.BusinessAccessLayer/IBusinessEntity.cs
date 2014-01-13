using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ED47.BusinessAccessLayer
{
    public interface IBusinessEntity
    {
        /// <summary>
        /// Gets the keys name and value of a business entity.
        /// </summary>
        /// <typeparam name="TEntity">The underlying DbEntity type of the business entity.</typeparam>
        IEnumerable<KeyValuePair<string, object>> GetKeys<TEntity>() where TEntity : DbEntity;

        void Commit();

        [IgnoreDataMember]
        EventProxy Events { get; set; }
    }
}