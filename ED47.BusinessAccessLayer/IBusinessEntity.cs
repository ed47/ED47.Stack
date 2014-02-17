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

        /// <summary>
        ///   Occurs before a primitive property change.
        /// </summary>
        event PropertyChangedEventHandler PropertyChange;

        void NotifyPropertyChange(PropertyChangedEventHandlerArgs args);

        /// <summary>
        /// Method to raise events as events can only be called within instance.
        /// </summary>
        /// <param name="args">The args.</param>
        void NotifyPropertyChanged(PropertyChangedEventHandlerArgs args);

        ClientData ClientData { get; set; }

        /// <summary>
        ///   Inits this instance. This method is executed after the database load and instance creation.
        /// </summary>
        void Init();
    }
}