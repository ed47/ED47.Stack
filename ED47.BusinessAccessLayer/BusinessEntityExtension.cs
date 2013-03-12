using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Omu.ValueInjecter;
namespace ED47.BusinessAccessLayer
{
   public static class BusinessEntityExtension
    {
       public static void Apply<TEntity, TData>(this IEnumerable<TEntity> entities, IEnumerable<TData> data, Func<TEntity, object> entitySelector = null, Func<TData, object> dataSelector = null)
       {
           var idPropInfo1 = typeof(TEntity).GetProperty("Id");
           var idPropInfo2 = typeof(TData).GetProperty("Id");
           var ks1 = entitySelector ?? (el => idPropInfo1 != null ? idPropInfo1.GetValue(el,null) : el);
           var ks2 = dataSelector ?? (el => idPropInfo2 != null ? idPropInfo2.GetValue(el,null) : el);
           
           var entitiesByKey = entities.GroupBy(ks1).ToDictionary(el=>el.Key);

           foreach (var o in data)
           {
               var current = entitiesByKey[ks2(o)];
               if (current == null) continue;
               foreach (var businessEntity in current)
               {
                   businessEntity.InjectFrom(o);
               }
           }
        }

       public static void Apply<TEntity>(this IEnumerable<TEntity> entities, SqlDataReader reader, Func<TEntity, object> entitySelector = null, Func<SqlDataReader, object> dataSelector = null) 
       {
           var idPropInfo1 = typeof(TEntity).GetProperty("Id");
           var ks1 = entitySelector ?? (el => idPropInfo1 != null ? idPropInfo1.GetValue(el, null) : el);
           var ks2 = dataSelector ?? (el => el.GetInt32(el.GetOrdinal("Id")));
           var entitiesByKey = entities.GroupBy(ks1).ToDictionary(el => el.Key);
           while (reader.Read())
           {
               var id = ks2(reader);
               var current = entitiesByKey[id];
               if (current == null) continue;

               foreach (var entity in current)
               {
                   entity.InjectFrom<ReaderInjection>(reader);
               }
           }
       }

        
    }
}
