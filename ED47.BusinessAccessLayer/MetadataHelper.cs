using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace ED47.BusinessAccessLayer
{
    public static class MetadataHelper
    {
        /// <summary>
        /// Gets the key members for an entity type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="dbContext">The Entity Framework Db Context.</param>
        /// <returns>The collection of key member names.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static IEnumerable<string> GetKeyMembers<TEntity>(IObjectContextAdapter dbContext) where TEntity : class
        {
            var objectContext = dbContext.ObjectContext;
            var set = objectContext.CreateObjectSet<TEntity>();
            var keyMembers = set.EntitySet.ElementType
                .KeyMembers
                .Select(k => k.Name);
            return keyMembers;
        }

        /// <summary>
        /// Gets the multilingual properties with values from an entity.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static IEnumerable<PropertyInfo> GetMultilingualProperties<TEntity>() where TEntity : class
        {
            //Get the real object type in case a proxy object is passed.
            var entityType = ObjectContext.GetObjectType(typeof(TEntity));
            var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            return properties.Where(p => p.GetCustomAttributes(inherit: false)
                                            .OfType<MultilingualPropertyAttribute>()
                                            .Any());
        }

        /// <summary>
        /// Generates the composite key for an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to get the key from</typeparam>
        /// <param name="entity">The entity instance to get the keys from.</param>
        /// <param name="dbContext">The Entity Framework DB Context.</param>
        /// <returns>The properly formated composite key.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static string GetKey<TEntity>(this TEntity entity, IObjectContextAdapter dbContext) where TEntity :  DbEntity
        {
            var keyMembers = GetKeyMembers<TEntity>(dbContext);
            var entityType = typeof(TEntity);
            var key = entityType.Name + "[";
            var hadPreviousKey = false;

            
            foreach(var keyMember in keyMembers)
            {
                if (hadPreviousKey)
                    key += ",";

                var keyValue = entityType.GetProperty(keyMember).GetValue(entity, null);
                key += keyValue;
                hadPreviousKey = true;
            }

            key += "]";
            return key;
        }

        /// <summary>
        /// Gets the collection of key values for an object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to get key values for.</typeparam>
        /// <param name="entity">The entity to get key values for.</param>
        /// <param name="keyMembers">The collection of keymember names.</param>
        /// <returns>A collection of key values.</returns>
       [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static IEnumerable<object> GetKeyValues<TEntity>(this TEntity entity, IEnumerable<string> keyMembers)
        {
            var entityType = entity.GetType();
			
            return keyMembers.Select(keyMember => entityType.GetProperty(keyMember).GetValue(entity, null));
        }
    }
}
