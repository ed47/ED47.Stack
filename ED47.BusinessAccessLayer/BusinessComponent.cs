using System.Collections.Generic;
using ED47.Settings;
using Ninject;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    ///   Represent a business component. This class is a singleton.
    /// </summary>
    public abstract class BusinessComponent
    {

        private static IKernel _kernel;

        /// <summary>
        /// IOC kernel for this application.
        /// </summary>
        public static IKernel Kernel
        {
            get { return _kernel ?? (_kernel = new StandardKernel()); }
        }


        private static readonly ICollection<BusinessComponent> _Components = new List<BusinessComponent>();
        private readonly ICollection<BusinessDomain> _Domains;

        /// <summary>
        ///   Initializes a new instance of the <see cref="BusinessComponent" /> class.
        /// </summary>
        protected BusinessComponent()
        {
            _Domains = new List<BusinessDomain>();
        }

    
        /// <summary>
        ///   Gets the components registered.
        /// </summary>
        public static ICollection<BusinessComponent> Components
        {
            get { return _Components; }
        }

        /// <summary>
        ///   Gets the domains belonging to this repository.
        /// </summary>
        public ICollection<BusinessDomain> Domains
        {
            get { return _Domains; }
        }

        /// <summary>
        ///   Start all components registered
        /// </summary>
        public static void StartAll()
        {


            foreach (var component in Components)
            {
                component.Start();
            }
        }


        /// <summary>
        ///   Registers a component instance in the application. This method should be called in the application start.
        /// </summary>
        /// <param name="component"> The component. </param>
        public static void RegisterComponent(BusinessComponent component)
        {
            if (component.Register())
                Components.Add(component);
        }

        /// <summary>
        ///   Registers this instance.
        /// </summary>
        /// <returns> </returns>
        public virtual bool Register()
        {
            return true;
        }


        /// <summary>
        ///   Registers a domain.
        /// </summary>
        /// <param name="domain"> The domain. </param>
        public void RegisterDomain(BusinessDomain domain)
        {
            if (domain.Register(this))
                _Domains.Add(domain);
        }

        /// <summary>
        ///   Starts this instance. Override this method to add custom behaviors.
        /// </summary>
        public virtual void Start()
        {
        }
    }
}