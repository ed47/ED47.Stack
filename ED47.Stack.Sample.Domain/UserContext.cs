using System;
using System.Web;
using ED47.BusinessAccessLayer;

namespace ED47.Stack.Sample.Domain
{
    public class UserContext : ED47.Stack.Sample.UserContext
    {
        //TODO: Change your context key here
        private const string DbContextKey = "ED47.Stack.Sample.Domain.UserContext.Instance.Repository";
        private Repository _current;

        protected override BusinessAccessLayer.Repository GetRepository()
        {
            return Repository;
        }
        
        private static UserContext _instance;
        public static new UserContext Instance
        {
            get
            {
                if (HttpContext.Current == null)
                    return _instance ?? (_instance = new UserContext());

                var userContext = Retrieve(InstanceKey) as UserContext;
                if (userContext == null)
                {
                    userContext = new UserContext();
                    Store(InstanceKey, userContext);
                }
                return userContext;
            }

        }

        /*
         * Sample user-specific property
         * Retrieve and Store put data in the HttpContext for the current call.
         * 
        public Person Person
        {
            get
            {
                var person = Retrieve("Person") as Person;

                if (person == null)
                {
                    person = Person.GetByUsername(UserName);
                    if (person == null) return null;

                    Store("Person", person);
                }

                return person;
            }
        }*/
    }
}
