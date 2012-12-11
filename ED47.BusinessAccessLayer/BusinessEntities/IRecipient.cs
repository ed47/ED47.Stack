using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public interface IRecipient
    {
        string Id { get; set; }
        string Email { get; set; }
        object SelectionObject { get; set; }
        string DisplayHtml { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
    }
}
