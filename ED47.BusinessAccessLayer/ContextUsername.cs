using System.Web;

namespace ED47.BusinessAccessLayer
{
    public static class ContextUsername
    {
        public static string Get()
        {
            if (HttpContext.Current == null || HttpContext.Current.User == null || !HttpContext.Current.User.Identity.IsAuthenticated)
                return "anonymous";

            return HttpContext.Current.User.Identity.Name;
        }
    }
}