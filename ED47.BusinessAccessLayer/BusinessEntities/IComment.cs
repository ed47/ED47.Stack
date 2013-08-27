using System;
using System.Collections.Generic;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public interface IComment
    {
        string BusinessKey { get; set; }
        string Body { get; set; }
        string Creator { get; set; }
        int CommenterId { get; set; }
        DateTime CreationDate { get; set; }
        IFileBox FileBox { get; set; }
        DateTime? DeletionDate { get; set; }
        DateTime? ModificationDate { get; set; }
        IEnumerable<IComment> Replies { get; }
        bool IsDeleted { get; set; }
        IComment Reply(string body, int commenterId, string creator = null, bool? encrypted = false);
        bool CanWrite();
        bool CanRead();
        bool CanDelete();
        bool CanReply();
        bool Delete();
        void AddFile(IFile file);
        int ParentLevel { get; set; }
        //event EventHandler OnAdd;
        //event EventHandler OnRemove;
        bool Edit(string body);
        IEnumerable<TComment> GetReplies<TComment>();
    }
}
