using System;
using System.IO;
using Afterthought;

namespace ED47.BusinessAccessLayer
{

    /// <summary>
    /// The business entity admender attribute. It injects property change events.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class BusinessEntityAmendmentAttribute : AmendmentAttribute
    {
        public BusinessEntityAmendmentAttribute()
            : base(typeof(BusinessEntityAmendment<>))
        {

        }
    }

    /// <summary>
    /// Amends Business Entities to raise events on primitive property changes.
    /// </summary>
    public class BusinessEntityAmendment<TType> : Amendment<TType, IBusinessEntity> where TType : IBusinessEntity
    {
        public override void Amend<TProperty>(Property<TProperty> property)
        {
            if (!property.Type.IsPrimitive && property.Type != typeof(String) && property.Type != typeof(DateTime) && property.Type != typeof(DateTime?))
            {
                if (property.Type.IsGenericType)
                {
                    var generic = property.Type.GetGenericArguments();

                    if (generic.Length == 0 || generic.Length > 1)
                        return;

                    if (!generic[0].IsPrimitive)
                        return;
                }
                else
                    return;
            }

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
        public static TProperty OnPropertyChange<TProperty>(IBusinessEntity instance, string propertyName, TProperty oldValue, TProperty newValue)
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
        public static void OnPropertyChanged<TProperty>(IBusinessEntity instance, string propertyName, TProperty oldValue, TProperty value, TProperty newValue)
        {
            instance.NotifyPropertyChanged(new PropertyChangedEventHandlerArgs { PropertyName = propertyName, Value = newValue });
        }
    }
}
