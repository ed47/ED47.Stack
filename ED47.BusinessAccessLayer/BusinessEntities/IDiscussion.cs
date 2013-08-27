using System.Collections.Generic;

namespace ED47.BusinessAccessLayer.BusinessEntities
{

    public interface IDiscussion : IComment
    {
        string Title { get; set; }
        bool IsEncrypted { get; set; }
        CommentOrder CommentOrder { get; set; }
    }
}
