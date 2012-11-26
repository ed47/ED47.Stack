using System;
using System.Collections.Generic;

namespace ED47.Stack.Reflector.Metadata
{
    /// <summary>
    /// Has a controller's metadata.
    /// </summary>
    public class ControllerInfo
    {
        /// <summary>
        /// Creates a new instance of a controller info.
        /// </summary>
        public ControllerInfo()
        {
            this.Actions = new List<ActionInfo>();
        }

        /// <summary>
        /// The controller's public actions.
        /// </summary>
        public List<ActionInfo> Actions { get; set; }

        /// <summary>
        /// The Type declaring the controller.
        /// </summary>
        public Type ControllerType { get; set; }

        /// <summary>
        /// The controller's kind.
        /// </summary>
        public ControllerKind Kind { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable client caching].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable client caching]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableClientCaching { get; set; }

        /// <summary>
        /// Flag indicated if the controller's library namespace should be used as Javascript namespace.
        /// </summary>
        public bool UseNamespace { get; set; }

        public string GetNamespace()
        {
            if (!this.UseNamespace)
                return String.Empty;

            var namespaces = this.ControllerType.Namespace.Split('.');
            if (namespaces.Length < 2)
                return "." + namespaces[0];

            return "." + namespaces[namespaces.Length - 2];
        }
    }
}
