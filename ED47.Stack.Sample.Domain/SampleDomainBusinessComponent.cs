using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ED47.BusinessAccessLayer;

namespace ED47.Stack.Sample.Domain
{
    public class SampleDomainBusinessComponent : BusinessComponent
    {
        private static SampleDomainBusinessComponent _current;

        public static SampleDomainBusinessComponent Current
        {
            get { return _current ?? (_current = new SampleDomainBusinessComponent()); }
        }
    }
}
