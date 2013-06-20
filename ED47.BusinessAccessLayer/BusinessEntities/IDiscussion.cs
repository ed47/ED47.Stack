using System;
using System.Collections.Generic;

namespace ED47.BusinessAccessLayer.BusinessEntities
{

    public interface IDiscussion : IComment
    {
        bool IsEncrypted { get; set; }
        CommentOrder CommentOrder { get; set; }
        HashSet<string> Notifiers { get; set; }
    }
}
