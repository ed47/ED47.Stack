using System;
using ED47.BusinessAccessLayer.BusinessEntities;
using ED47.BusinessAccessLayer.Message;
using Ninject;

namespace ED47.BusinessAccessLayer.EF
{
    public class EmailRepository : IEmailRepository
    {
        public IEmail Get(Guid id)
        {
            return BusinessComponent.Kernel.Get<BaseUserContext>().Repository.Find<Entities.Email, Email>(el => el.Guid == id);
        }
    }
}
