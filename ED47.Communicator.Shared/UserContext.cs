using System;
using System.Web;
using ED47.BusinessAccessLayer;

namespace ED47.Communicator.Shared
{
    /// <summary>
    /// The base UserContext for the application.
    /// </summary>
    public abstract class UserContext : BaseUserContext
    {
        internal protected new Repository Repository { get { return base.Repository; } }
        public new static Func<UserContext> CreateDefaultContext { get; set; }

        public new static UserContext Instance
        {
            get
            {
                var userContext = Retrieve(InstanceKey) as UserContext;
                if (userContext == null && CreateDefaultContext != null)
                {
                    userContext = CreateDefaultContext();
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

        public string LanguageCode
        {
            get
            {
                var languageCode = Retrieve("LanguageCode") as string;

                if (languageCode == null)
                {
                    var cookie = HttpContext.Current.Request.Cookies["LanguageCode"];

                    if (cookie == null)
                    {
                        //TODO: Get the language from the user profile or something
                    }
                    else
                    {
                        languageCode = cookie.Value;
                    }

                    Store("LanguageCode", languageCode);
                }

                return languageCode;
            }
        }
    }
}
