using System;
using System.Collections.Generic;
using System.Linq;


namespace ED47.BusinessAccessLayer.BusinessEntities.CouchBase.Comment
{
    public class Comment : IComment
    {
        protected static readonly ICollection<CommentNotifier> Notifiers = new List<CommentNotifier>();

        public static void AddNotifier(CommentNotifier notifier)
        {
            Notifiers.Add(notifier);
        }

        public string BusinessKey { get; set; }
        public string Body { get; set; }
        public string Creator { get; set; }

        public int CommenterId { get; set; }

        public DateTime CreationDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletionDate { get; set; }

        public DateTime? ModificationDate { get; set; }

        protected  List<Comment> Comments = new List<Comment>();
        public IEnumerable<IComment> Replies
        {
            get { return Comments; }
        }
        private IFileBox _fileBox;

        public IFileBox FileBox
        {
            get { return _fileBox ?? (_fileBox = CouchBase.Comment.FileBox.CreateNew("comment")); }
            set { _fileBox = value; }
        }

        public static IComment Create(string body, int commenterId, string creator = null, bool? encrypted = false)
        {
            IComment newComment;

            if(encrypted == null || !encrypted.Value)
                newComment = new Comment();
            else
                newComment = new EncryptedComment();

            newComment.BusinessKey = Guid.NewGuid().ToString();
            newComment.Body = body.Trim();
            newComment.Creator = creator;
            newComment.CreationDate = DateTime.UtcNow;
            newComment.CommenterId = commenterId;
            return newComment;
        }

        public void AddFiles(IEnumerable<int> fileIds)
        {
            if (fileIds == null)
                return;

            foreach (var fileId in fileIds)
            {
                var file = File.Get(fileId);

                if (file != null)
                {
                    FileBox.AddFile(file);
                }
            }
        }

        /// <summary>
        /// Add files from an existing list of FileBoxItems to the comment.
        /// </summary>
        /// <param name="files">The FileBoxItems to add to the comment.</param>
        public void AddFiles(IEnumerable<IFileBoxItem> files)
        {
            foreach (var fileBoxItem in files)
            {
                FileBox.AddFile(fileBoxItem.File);
            }
        }

        public virtual IComment Reply(string comment, int commentId, string creator = null, bool? encrypted = false)
        {
            if (!CanReply()) return null;            
            var newComment = Create(comment, commentId, creator,encrypted);
            Comments.Add((Comment)newComment);
            return newComment;
        }

        public virtual bool CanWrite()
        {
            return Replies.All(el => el.IsDeleted) && !IsDeleted;
        }

        public virtual bool CanRead()
        {
            return !IsDeleted;
        }

        public virtual bool CanDelete()
        {
            //TODO : add condition if comment creator == current user or admin
            return Replies.All(el => el.IsDeleted);
        }

        public virtual bool CanReply()
        {
            //TODO : add condition if comment creator != current user
            
            return !IsDeleted;
        }

        public virtual bool Delete()
        {
            if (CanDelete())
            {
                IsDeleted = true;
                return true;
            }

            return false;
        }

        public void AddFile(IFile file)
        {
            FileBox.AddFile(file);
        }

        public int ParentLevel { get; set; }

        public bool Edit(string body)
        {
            if (CanWrite())
            {
                Body = body;
                ModificationDate = DateTime.UtcNow;
                return true;
            }
            return false;
        }
    }

    public class EncryptedComment : Comment
    {
        [EncryptedField]
        public  string Body
        {
            get
            {
                return base.Body;
            }
            set
            {
                base.Body = value;
            }
        }
    }
    
}
