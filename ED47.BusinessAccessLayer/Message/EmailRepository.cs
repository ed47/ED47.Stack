using System;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer.Message
{
    public static class EmailRepository
    {
        public static IEmail Get(Guid id)
        {
            return EmailFactory.Default.Get(id);
        }
    }
}
