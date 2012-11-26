using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Reflection;
namespace ED47.BusinessAccessLayer
{
    public static class EnumerableExtension
    {
        public static DataTable ToDataTable<TSource>(this IEnumerable<TSource> collection)
        {
            var table = new DataTable();
            var type = typeof(TSource);
            var props = type.GetProperties();
            foreach (var prop in props.Where(el=>el.CanRead))
            {
                table.Columns.Add(prop.Name);
            }


            foreach (var o in collection)
            {
                var row = table.NewRow();
                foreach (var prop in props)
                {
                    row[prop.Name] = prop.GetValue(o, null);
                }
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
