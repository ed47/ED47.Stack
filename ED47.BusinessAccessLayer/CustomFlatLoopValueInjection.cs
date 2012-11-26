using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Omu.ValueInjecter;

namespace ED47.BusinessAccessLayer
{
    public class CustomFlatLoopValueInjection : LoopValueInjectionBase
    {
        protected override void Inject(object source, object target)
        {
            if (source == null)
                return;

            foreach (PropertyDescriptor targetProperty in target.GetProps())
            {
                var t1 = targetProperty;
                var es = UberFlatter.Flat(targetProperty.Name, source, type => TypesMatch(type,t1.PropertyType));

                var sourceType = source.GetType();

                if (!es.Any() && !sourceType.IsPrimitive && targetProperty.PropertyType != typeof(String) && !targetProperty.PropertyType.IsInterface)
                {
                    var sourceProperty = source.GetType().GetProperty(targetProperty.Name);
                    if (sourceProperty != null)
                    {
                        var sourceValue = sourceProperty.GetValue(source, null);
                        if (sourceValue != null)
                        {
                            object targetValue = null;
                            if(sourceProperty.PropertyType.IsSubclassOf(typeof(DbEntity))
                                && targetProperty.PropertyType.IsSubclassOf(typeof(BusinessEntity)))
                            {
                                var idprop = sourceProperty.PropertyType.GetProperty("Id");
                                if(idprop!= null && idprop.PropertyType == typeof(Int32))
                                {
                                    targetValue = BaseUserContext.TryGetDynamicInstance(targetProperty.PropertyType,
                                                                                  Convert.ToInt32(idprop.GetValue(sourceValue, null)));
                                }
                            }
                            if (targetValue == null)
                            {
                                targetValue = Activator.CreateInstance(targetProperty.PropertyType);
                                if(targetValue != null)
                                    Inject(sourceValue, targetValue);
                                if (targetValue is BusinessEntity)
                                    BaseUserContext.StoreDynamicInstance(targetProperty.PropertyType,targetValue as BusinessEntity);
                            }
                            targetProperty.SetValue(target, targetValue);
                        }
                    }
                }

                Debug.Assert(es != null, "es != null");
                var endpoint = es.FirstOrDefault(el => el != null);
                if (endpoint == null) continue;
                var val = endpoint.Property.GetValue(endpoint.Component);

                if (AllowSetValue(val))
                    targetProperty.SetValue(target, SetValue(val));

            }
        }

        protected virtual bool TypesMatch(Type sourceType, Type targetType)
        {
            return targetType == sourceType;
        }

        protected virtual object SetValue(object sourcePropertyValue)
        {
            return sourcePropertyValue;
        }
    }


    public abstract class FlatLoopValueInjection<TSourceProperty, TTargetProperty> : LoopValueInjectionBase
    {
        protected override void Inject(object source, object target)
        {
            foreach (PropertyDescriptor t in target.GetProps())
            {
                if (t.PropertyType != typeof (TTargetProperty)) continue;

                var values = UberFlatter.Flat(t.Name, source, type => type == typeof (TSourceProperty));

                if (!values.Any()) continue;

                var val = values.First().Property.GetValue(values.First().Component);

                if (AllowSetValue(val))
                    t.SetValue(target, SetValue((TSourceProperty) val));
            }
        }

        protected abstract TTargetProperty SetValue(TSourceProperty sourceValues);
    }
}
