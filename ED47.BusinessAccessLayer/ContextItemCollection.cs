using System.Collections;
using System.Web;

namespace ED47.BusinessAccessLayer
{
    public static class ContextItemCollection
    {
        public static IDictionary GetItems()
        {
            if (HttpContext.Current == null)
                return null;

            return HttpContext.Current.Items;
        }
    }
}
