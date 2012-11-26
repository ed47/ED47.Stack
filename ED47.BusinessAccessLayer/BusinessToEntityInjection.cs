using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omu.ValueInjecter;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Injects client sent data to business entities. A whitelist of fields to inject can be provided to limit injection.
    /// </summary>
    public class BusinessToEntityInjection : ConventionInjection
    {
        
        protected override bool Match(ConventionInfo c)
        {
            return (c.SourceProp.Name == c.TargetProp.Name && c.SourceProp.Type == c.TargetProp.Type);
        }
    }
}
