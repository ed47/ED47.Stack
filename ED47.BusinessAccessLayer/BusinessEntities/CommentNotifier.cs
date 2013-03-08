using System;
using System.Text.RegularExpressions;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class CommentNotifier
    {
        public String Regex { get; set; }
        public Action< Comment, Match ,CommentActionType> Action { get; set; }
       

        internal bool TryNotify(Comment comment, CommentActionType actionType)
        {
            var m = new Regex(Regex).Match(comment.BusinessKey);
            if (Action == null || !m.Success) return false;

            Action(comment, m, actionType);
            return true;
        }
    }
}