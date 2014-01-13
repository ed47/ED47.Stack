using System;
using System.Collections.Concurrent;

namespace ED47.BusinessAccessLayer
{
    public class EventProxy
    {
        private static ConcurrentDictionary<Type, EventProxy> _proxies;
        internal static ConcurrentDictionary<Type, EventProxy> Proxies
        {
            get { return _proxies ?? (_proxies = new ConcurrentDictionary<Type, EventProxy>()); }
        }

        /// <summary>
        /// Registers an event proxy on a Type.
        /// </summary>
        /// <param name="type">The type to register the event proxy to.</param>
        /// <param name="proxy">The proxy event to register.</param>
        public static void Register(Type type, EventProxy proxy)
        {
            Proxies[type] = proxy;
        }

        /// <summary>
        /// Gets an event proxy registeres to a Type. If the Type doesn't have a registered proxy, a new one of the requested type will be registered.
        /// </summary>
        /// <typeparam name="TEventProxy">The event proxy type to get.</typeparam>
        /// <param name="type">The type to get the event proxy for.</param>
        /// <returns>The Type's event proxy.</returns>
        public static TEventProxy GetSingleton<TEventProxy>(Type type) where TEventProxy : EventProxy, new()
        {
            lock (Lock.Get(type.FullName))
            {
                EventProxy baseProxy;
                TEventProxy proxy = null;

                if (Proxies.TryGetValue(type, out baseProxy))
                    proxy = baseProxy as TEventProxy;

                if (proxy == null && !Proxies.ContainsKey(type))
                {
                    proxy = new TEventProxy();
                    Register(type, proxy);
                }

                return proxy;
            }
        }


        public static TEventProxy Get<TEventProxy>(Type type) where TEventProxy : EventProxy, new()
        {
            lock (Lock.Get(type.FullName))
            {
                return new TEventProxy();
            }
        }

        public event EventHandler<EventArgs> Update;
        public static bool NotifyUpdate<TBusinessEntity>(TBusinessEntity sender) where TBusinessEntity : IBusinessEntity
        {
            var proxy = GetSingleton<EventProxy>(typeof (TBusinessEntity));

            if (proxy == null) return true;

            if (proxy.Update != null)
                proxy.Update(sender, new EventArgs());
            
            if(sender.Events != null && sender.Events.Update != null)
                sender.Events.Update(sender,new EventArgs());


            return true;
        }
       
      

        public event EventHandler<EventArgs> Updated;

        public static bool NotifyUpdated<TBusinessEntity>(TBusinessEntity sender) where TBusinessEntity : IBusinessEntity
        {
            var proxy = GetSingleton<EventProxy>(typeof (TBusinessEntity));

            if (proxy == null) return true;

            if (proxy.Updated != null)
                proxy.Updated(sender, new EventArgs());

            if (sender.Events != null && sender.Events.Updated != null)
                sender.Events.Updated(sender, new EventArgs());

            return true;
        }

        public event EventHandler<EventArgs> Add;

        public static bool NotifyAdd<TBusinessEntity>(TBusinessEntity sender) where TBusinessEntity : IBusinessEntity
        {
            var proxy = GetSingleton<EventProxy>(typeof (TBusinessEntity));

            if (proxy == null) return true;

            if (proxy.Add != null)
                proxy.Add(sender, new EventArgs());

            if (sender.Events != null && sender.Events.Add != null)
                sender.Events.Add(sender, new EventArgs());

            return true;
        }

        public event EventHandler<EventArgs> Added;

        public static bool NotifyAdded<TBusinessEntity>(TBusinessEntity sender) where TBusinessEntity : IBusinessEntity
        {
            var proxy = GetSingleton<EventProxy>(typeof (TBusinessEntity));

            if (proxy == null) return true;

            if (proxy.Added != null)
                proxy.Added(sender, new EventArgs());

            if (sender.Events != null && sender.Events.Added != null)
                sender.Events.Added(sender, new EventArgs());

            return true;
        }

        public event EventHandler<CancellableRepositoryEventArgs> Delete;

        public static bool NotifyDelete<TBusinessEntity>(TBusinessEntity sender) where TBusinessEntity : IBusinessEntity
        {
            var proxy = GetSingleton<EventProxy>(typeof (TBusinessEntity));

            if (proxy == null) return true;
            var eventArgs = new CancellableRepositoryEventArgs();

            if (proxy.Delete != null)
                proxy.Delete(sender, eventArgs);
            
            if (sender.Events != null && sender.Events.Delete != null)
                sender.Events.Delete(sender, eventArgs);

            return !eventArgs.Cancel;
        }

        public event EventHandler<EventArgs> Deleted;

        public static bool NotifyDeleted<TBusinessEntity>(TBusinessEntity sender) where TBusinessEntity : IBusinessEntity
        {
            var proxy = GetSingleton<EventProxy>(typeof (TBusinessEntity));

            if (proxy == null) return true;

            if (proxy.Deleted != null)
                proxy.Deleted(sender, new EventArgs());

            if (sender.Events != null && sender.Events.Deleted != null)
                sender.Events.Deleted(sender, new EventArgs());

            return true;
        }

    }
}
