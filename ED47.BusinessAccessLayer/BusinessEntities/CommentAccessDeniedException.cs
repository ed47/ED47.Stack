using System;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class CommentAccessDeniedException : Exception
    {
        public CommentAccessDeniedException(string businessKey, string username)
        {
            BusinessKey = businessKey;
            Username = username;
        }

        public string Username { get; private set; }

        public override string Message
        {
            get
            {
                return String.Format("Access to comment denied: business key '{0}', username '{1}'", BusinessKey, Username);
            }
        }

        public string BusinessKey { get; private set; }
    }
}