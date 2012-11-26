using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Represent a business domain inside a business component. 
    /// This class is a singleton.
    /// </summary>
    public abstract class BusinessDomain
    {
        public BusinessComponent Component { get; set; }

        /// <summary>
        /// Registers the specified business component.
        /// </summary>
        /// <param name="businessComponent">The business component.</param>
        /// <returns></returns>
        public virtual bool Register(BusinessComponent businessComponent)
        {
            Component = businessComponent;
            return true;
        }

        /// <summary>
        /// Starts this domain. Override this method to add custom behaviors.
        /// </summary>
        public virtual void Start()
        {
            
        }
    }
}
