using ED47.BusinessAccessLayer;

namespace ED47.Communicator.Admin
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
