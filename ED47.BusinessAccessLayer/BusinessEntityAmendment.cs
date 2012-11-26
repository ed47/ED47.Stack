using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Afterthought;

namespace ED47.BusinessAccessLayer
{

    /// <summary>
    /// The business entity admender attribute. It injects property change events.
    /// </summary>
    public class BusinessEntityAmendmentAttribute : AmendmentAttribute
    {
        public BusinessEntityAmendmentAttribute()
            : base(typeof(BusinessEntityAmendment<>))
        {
            
        }
    }

    /// <summary>
    /// Amends Business Entities to raise events on primitive property changes.
    /// </summary>
    public class BusinessEntityAmendment<TType> : Amendment<TType, BusinessEntity> where TType : BusinessEntity
    {
        public override void Amend<TProperty>(Property<TProperty> property)
        {
            if (property.Type.IsPrimitive) return;

            property.BeforeSet = OnPropertyChange<TProperty>;
            property.AfterSet = OnPropertyChanged<TProperty>;
        }


        /// <summary>
        /// Called when [property change].
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        public static TProperty OnPropertyChange<TProperty>(BusinessEntity instance, string propertyName, TProperty oldValue, TProperty newValue)
        {
            instance.NotifyPropertyChange(new PropertyChangedEventHandlerArgs { PropertyName = propertyName, Value = newValue });
            return newValue;
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="value">The value.</param>
        /// <param name="newValue">The new value.</param>
        public static void OnPropertyChanged<TProperty>(BusinessEntity instance, string propertyName, TProperty oldValue, TProperty value, TProperty newValue)
        {
            instance.NotifyPropertyChanged(new PropertyChangedEventHandlerArgs { PropertyName = propertyName, Value = newValue });
        }
    }
}
