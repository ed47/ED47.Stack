using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ED47.BusinessAccessLayer
{
    public abstract class BusinessEntity
    {
        private BusinessEntityTracker _Tracker;

        [IgnoreDataMember]
        public  EventProxy Events { get; set; }

        [IgnoreDataMember]
        public ClientData ClientData { get; set; }
        /// <summary>
        ///   Inits this instance. This method is executed after the database load and instance creation.
        /// </summary>
        public virtual void Init()
        {
            _Tracker = new BusinessEntityTracker(this);
        }

        /// <summary>
        ///   Commits this instance by reseting the change tracker.
        /// </summary>
        internal void Commit()
        {
            if(_Tracker != null)
                _Tracker.Reset();
        }

        /// <summary>
        ///   Determines whether the specified property has changed.
        /// </summary>
        /// <param name="propertyName"> Name of the property. </param>
        /// <returns> <c>true</c> if the specified property has changed; otherwise, <c>false</c> . </returns>
        public bool HasChanged(string propertyName)
        {
            return _Tracker.HasChanged(propertyName);
        }

        /// <summary>
        /// Gets the initial value of a property.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public TValue GetInitialValue<TValue>(string propertyName)
        {
            return _Tracker.GetInitValue<TValue>(propertyName);
        }

        /// <summary>
        ///   Occurs after a primitive property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///   Occurs before a primitive property change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChange;

        /// <summary>
        /// Method to raise events as events can only be called within instance.
        /// </summary>
        /// <param name="args">The args.</param>
        public void NotifyPropertyChanged(PropertyChangedEventHandlerArgs args)
        {
            if(PropertyChanged!=null)
                PropertyChanged(this, args);
        }

        /// <summary>
        /// Method to raise events as events can only be called within instance.
        /// </summary>
        /// <param name="args">The args.</param>
        public void NotifyPropertyChange(PropertyChangedEventHandlerArgs args)
        {
            if (PropertyChange != null)
                PropertyChange(this, args);
        }

        /// <summary>
        /// Property indicating if the entity is valid.
        /// Always returns true unless overriden.
        /// </summary>
        public virtual bool IsValid
        {
            get { return true; }
        } 


        /// <summary>
        /// Gets the keys name and value of a business entity.
        /// </summary>
        /// <typeparam name="TEntity">The underlying DbEntity type of the business entity.</typeparam>
        /// <returns></returns>
        

        public IEnumerable<KeyValuePair<string, object>> GetKeys<TEntity>() where TEntity : DbEntity
        {
            var entityType = GetType();
            var keyMembers = MetadataHelper.GetKeyMembers<TEntity>(BaseUserContext.Instance.Repository.DbContext);
            return keyMembers.Select(k => new KeyValuePair<string, object>(k, entityType.GetProperty(k).GetValue(this, null)));
        }
        
        /// <summary>
        /// Gets a copy of all the changes.
        /// </summary>
        public Dictionary<string, object> GetAllChanges()
        {
            return _Tracker.GetAllChanges();
        }
    }
}