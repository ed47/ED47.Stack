using System;
using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Injects client sent data to business entities. A whitelist of fields to inject can be provided to limit injection.
    /// </summary>
    public class ClientToBusinessInjection : LoopValueInjection
    {
        public ICollection<string> AllowedFields { get; set; }
        public ICollection<string> DeniedFields { get; set; }

        /// <summary>
        /// Makes an injection that only injects allowed fields.
        /// </summary>
        /// <param name="allowedFields">The list of the names of the allowed fields.</param>
        /// /// <param name="deniedFields">The list of the names of the denied fields.</param>
        public ClientToBusinessInjection(ICollection<string> allowedFields = null, ICollection<string> deniedFields = null)
        {
            this.AllowedFields = allowedFields;
            this.DeniedFields = deniedFields;
        }

        protected override bool UseSourceProp(string sourcePropName)
        {
            return (this.AllowedFields == null || this.AllowedFields.Contains(sourcePropName)) && (DeniedFields == null || !DeniedFields.Contains(sourcePropName));
        }

        protected override bool TypesMatch(Type sourceType, Type targetType)
        {
            return sourceType == targetType ||
                   (sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(Nullable<>) && sourceType.GetGenericArguments().First() == targetType);
        }
    }
}
