using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Omu.ValueInjecter;

namespace ED47.BusinessAccessLayer
{
    public class ReaderInjection : KnownSourceValueInjection<IDataReader>
    {
        public static IEnumerable<TBusinessEntity> ReadAll<TBusinessEntity>(IDataReader reader) where TBusinessEntity : class
        {
            var injecter = new ReaderInjection();
            
            while (reader.Read())
            {
                var obj = Activator.CreateInstance(typeof (TBusinessEntity)) as TBusinessEntity;
                injecter.Inject(reader,obj);
                yield return obj;
            }
        }
        
        protected override void Inject(IDataReader source, object target)
        {
            for (var i = 0; i < source.FieldCount; i++)
            {
                var targetProp = target.GetProps().GetByName(source.GetName(i), true);
                if (targetProp == null) continue;

                var value = source.GetValue(i);
                if (value == DBNull.Value) value = null;

                targetProp.SetValue(target, value);
            }
        }
    }

}
