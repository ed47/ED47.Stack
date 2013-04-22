using i18n = ED47.Stack.Web.Multilingual.Multilingual;

namespace ED47.Stack.Sample
{
    /// <summary>
    /// Contains the default error messages for fluent validation using the multilingual system. 
    /// </summary>
    public class DefaultValidationMessages
    {
        /*TODO: 
         * 1- Add your default validation messages to a translation file in /App_data/Translations/XXX.xml
         * 2- replace the translation keys used down here with yours
        */

        public static string email_error
        {
            get { return i18n.N("sample.validation.InvalidEmail"); }
        }

        public static string equal_error
        {
            get { return i18n.N("sample.validation.Equality"); }
        }

        public static string length_error
        {
            get { return i18n.N("sample.validation.Length"); }
        }

        public static string exact_length_error
        {
            get { return i18n.N("sample.validation.ExactLength"); }
        }

        public static string notempty_error
        {
            get { return i18n.N("sample.validation.Required"); }
        }
        public static string notnull_error
        {
            get { return i18n.N("sample.validation.NotNull"); }
        }

        public static string lessthanorequal_error
        {
            get { return i18n.N("sample.validation.LEssThanOrEqual"); }
        }

        public static string notequal_error
        {
            get { return i18n.N("sample.validation.NotEqual"); }
        }

        public static string exclusivebetween_error
        {
            get { return i18n.N("sample.validation.Exclusive"); }
        }

        public static string greaterthanorequal_error
        {
            get { return i18n.N("sample.validation.GreaterOrEqual"); }
        }

        public static string greaterthan_error
        {
            get { return i18n.N("sample.validation.Greate"); }
        }

        public static string inclusivebetween_error
        {
            get { return i18n.N("sample.validation.Length"); }
        }

        public static string lessthan_error
        {
            get { return i18n.N("sample.validation.PasswordLength"); }
        }

        public static string predicate_error
        {
            get
            {
                return i18n.N("sample.validation.Predicate");
            }
        }

        public static string regex_error
        {
            get
            {
                return i18n.N("sample.validation.Regex");
            }
        }
    }
}