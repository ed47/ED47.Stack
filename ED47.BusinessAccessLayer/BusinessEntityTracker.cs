using System;
using System.Collections.Generic;
using System.Linq;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    ///   Track the changes on property on <see cref="BusinessEntity" /> object
    /// </summary>
    internal class BusinessEntityTracker
    {
        private readonly IBusinessEntity _trackedEntity;
        private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

        /// <summary>
        ///   Initializes a new instance of the <see cref="BusinessEntityTracker" /> class.
        /// </summary>
        /// <param name="trackedEntity"> The tracked entity. </param>
        internal BusinessEntityTracker(IBusinessEntity trackedEntity)
        {
            _trackedEntity = trackedEntity;

            TrackedEntity.PropertyChange += TrackedEntityPropertyChange;
        }

        /// <summary>
        ///   Gets the tracked entity.
        /// </summary>
        internal IBusinessEntity TrackedEntity
        {
            get { return _trackedEntity; }
        }

        /// <summary>
        ///   Trackeds the entity property change to get the initial value
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="eventArgs"> The event args. </param>
        private void TrackedEntityPropertyChange(object sender, PropertyChangedEventHandlerArgs eventArgs)
        {
            if (sender != TrackedEntity) return;

            if (!_values.ContainsKey(eventArgs.PropertyName))
                _values[eventArgs.PropertyName] = GetCurrentValue(eventArgs.PropertyName);
        }


        /// <summary>
        ///   Determines whether the specified property value has changed.
        /// </summary>
        /// <param name="propertyName"> Name of the property. </param>
        /// <returns> <c>true</c> if the specified property value has changed; otherwise, <c>false</c> . </returns>
        internal bool HasChanged(string propertyName)
        {
            return (_values.ContainsKey(propertyName) && GetInitValue(propertyName) != (GetCurrentValue(propertyName)));
        }


        /// <summary>
        ///   Gets the current value for specified property of the tracked object.
        /// </summary>
        /// <param name="propertyName"> Name of the property. </param>
        /// <returns> The value of the property </returns>
        internal object GetCurrentValue(string propertyName)
        {
            return GetCurrentValue<object>(propertyName);
        }

        /// <summary>
        ///   Gets the current value for specified property of the tracked object.
        /// </summary>
        /// <typeparam name="TValue"> The type of the value. </typeparam>
        /// <param name="propertyName"> Name of the property. </param>
        /// <returns> The value of the property </returns>
        internal TValue GetCurrentValue<TValue>(string propertyName)
        {
            var type = TrackedEntity.GetType();
            var pinfo = type.GetProperty(propertyName);

            if (pinfo == null)
                throw new InvalidOperationException(String.Format("The property {0} doesn't belong to type {1}",
                                                                  propertyName, type.FullName));

            return (TValue) pinfo.GetValue(TrackedEntity, null);
        }


        /// <summary>
        ///   Gets the initial value of a property of the tracked object.
        /// </summary>
        /// <param name="propertyName"> Name of the property. </param>
        /// <returns> The initial value </returns>
        internal object GetInitValue(string propertyName)
        {
            return GetInitValue<object>(propertyName);
        }

        /// <summary>
        ///   Gets the initial value of a property of the tracked object.
        /// </summary>
        /// <typeparam name="TValue"> The type of the value. </typeparam>
        /// <param name="propertyName"> Name of the property. </param>
        /// <returns> </returns>
        internal TValue GetInitValue<TValue>(string propertyName)
        {
            var type = TrackedEntity.GetType();
            var pinfo = type.GetProperty(propertyName);

            if (pinfo == null)
                throw new InvalidOperationException(String.Format("The property {0} doesn't belong to type {1}",
                                                                  propertyName, type.FullName));
            if (!_values.ContainsKey(propertyName) && pinfo.CanRead)
                return (TValue) pinfo.GetValue(TrackedEntity, null);


            return (TValue) _values[propertyName];
        }


        /// <summary>
        ///   Resets all the intitial values.
        /// </summary>
        internal void Reset()
        {
            _values.Clear();
        }

        /// <summary>
        /// Returns a dictionary with a copy of all the changes.
        /// </summary>
        public IDictionary<string, object> GetAllChanges()
        {
            return _values.ToDictionary(el => el.Key, el => el.Value);
        }
    }
}