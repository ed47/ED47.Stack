using System.Text.RegularExpressions;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class CommentNotifier : Notifier<Comment, CommentActionType>
    {       
        public bool TryNotify(string businessKey, CommentActionType actionType)
        {
            var m = new Regex(Regex).Match(businessKey);
            if (Action == null || !m.Success) return false;

            Action(null, m, actionType);
            return true;
        }
    }
}