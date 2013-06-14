using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public interface IComment
    {
        int Id { get; set; }
        string BusinessKey { get; set; }
        string Body { get; set; }
        int? CommenterId { get; set; }
        DateTime CreationDate { get; set; }
        int? FileBoxId { get; set; }
        bool IsReadOnly { get; set; }
        bool IsDeleted { get; set; }
        DateTime? DeletionDate { get; set; }
        string CommentType { get; set; }
    }
}
