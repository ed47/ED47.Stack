using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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
        public DateTime CreationDate { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletionDate { get; set; }
        List<Comment> Comments = new List<Comment>();
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

        public static IComment Create(string body, string creator = null, bool? encrypted = false)
        {
            Comment newComment;

            if(encrypted == null || !encrypted.Value)
                newComment = new Comment();
            else
                newComment = new EncryptedComment();

            newComment.BusinessKey = Guid.NewGuid().ToString();
            newComment.Body = body.Trim();
            newComment.Creator = creator;
            newComment.CreationDate = DateTime.UtcNow;
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

        public IComment Reply(string comment, string creator = null, bool? encrypted = false)
        {
            if (!CanReply()) return null;            
            var newComment = Create(comment, creator,encrypted);
            Comments.Add((Comment)newComment);
            return newComment;
        }

        public bool CanWrite()
        {
            return !Replies.Any() && !IsDeleted;
        }

        public bool CanRead()
        {
            return !IsDeleted;
        }

        public bool CanDelete()
        {
            return true;
        }

        public bool CanReply()
        {
            return !IsDeleted;
        }

        public bool Delete()
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
