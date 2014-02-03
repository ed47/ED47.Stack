using System;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer.Message
{
    public interface IEmailRepository
    {
        IEmail Get(Guid id);
    }
}