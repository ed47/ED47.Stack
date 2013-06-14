using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public enum CommentActionType
    {
        New,
        Delete,
        Edit,
        View
    }

    /// <summary>
    /// Represents a user comment.
    /// </summary>
    public class Comment : BusinessEntity
    {
        protected static readonly ICollection<CommentNotifier> Notifiers = new List<CommentNotifier>();

        public static void AddNotifier(CommentNotifier notifier)
        {
            Notifiers.Add(notifier);
        }

        public virtual int Id { get; set; }
        [MaxLength(250)]
        public virtual string BusinessKey { get; set; }
        public virtual string Body { get; set; }
        public virtual int? CommenterId { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual int? FileBoxId { get; set; }
        public virtual bool IsReadOnly { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual DateTime? DeletionDate { get; set; }
        public virtual string CommentType { get; set; }

        /// <summary>
        /// Returns the comments by their business key.
        /// </summary>
        /// <param name="businessKey">The business key to get comments for.</param>
        [Obsolete("User Get() instead.")]
        public static IEnumerable<Comment> GetByBusinessKey(string businessKey)
        {
            Notifiers.ToList().ForEach(el => el.TryNotify(businessKey, CommentActionType.View));

            return BaseUserContext.Instance.Repository
                    .Where<Entities.Comment, Comment>(el => el.BusinessKey == businessKey, new[] { "FileBox" })
                    .OrderByDescending(el => el.CreationDate)
                    .ToList();
        }

        public static Comment Create(string businessKey, string comment, int? commenterId = null, IEnumerable<int> fileIds = null, bool? encrypted = false)
        {
            Comment newComment;

            if(encrypted == null || !encrypted.Value)
                newComment = new Comment();
            else
                newComment = new EncryptedComment();

            newComment.BusinessKey = businessKey;
            newComment.Body = comment.Trim();
            newComment.CommenterId = commenterId;

            if (encrypted == null || !encrypted.Value)
                BaseUserContext.Instance.Repository.Add<Entities.Comment, Comment>(newComment);
            else
                BaseUserContext.Instance.Repository.Add<Entities.Comment, EncryptedComment>((EncryptedComment)newComment);

            newComment.AddFiles(fileIds);
            Notifiers.ToList().ForEach(el => el.TryNotify(newComment, CommentActionType.New));
            MakePreviousReadOnly(newComment.Id, newComment.BusinessKey);
         

            return newComment;
        }

        private static void MakePreviousReadOnly(int currentCommentId, string businessKey)
        {
            BaseUserContext.Instance.Repository.MakePreviousCommentReadOnly(currentCommentId, businessKey);
        }

        private void AddFiles(IEnumerable<int> fileIds)
        {
            if (fileIds == null)
                return;

            var filebox = GetOrCreateFileBox();

            foreach (var fileId in fileIds)
            {
                var file = File.Get(fileId);

                if (file == null) 
                    continue;

                FileBoxItem.CreateNew(filebox.Id, file);
            }
        }

        /// <summary>
        /// Add files from an existing list of FileBoxItems to the comment.
        /// </summary>
        /// <param name="files">The FileBoxItems to add to the comment.</param>
        public void AddFiles(IEnumerable<FileBoxItem> files)
        {
            var filebox = GetOrCreateFileBox();

            foreach (var fileBoxItem in files)
            {
                FileBoxItem.CreateNew(filebox.Id, fileBoxItem.File);
            }
        }

        private FileBox GetOrCreateFileBox()
        {
            FileBox filebox;

            if (FileBoxId == null)
            {
                filebox = BusinessEntities.FileBox.CreateNew("Comment");
                this.FileBoxId = filebox.Id;
                this.Save();
            }
            else
                filebox = this.FileBox;
            return filebox;
        }

        private FileBox _fileBox;
        public FileBox FileBox
        {
            get { return _fileBox ?? (_fileBox = FileBoxId.HasValue ? FileBox.Get(FileBoxId.Value) : null); }
        }


        public void OnCommentSaved(object sender, EventArgs e)
        {
            BaseUserContext.Instance.Commited -= OnCommentSaved;
            Notifiers.ToList().ForEach(el => el.TryNotify(this, CommentActionType.Edit));
        }
            

        public virtual void Save()
        {
            BaseUserContext.Instance.Repository.Update<Entities.Comment, Comment>(this);
            BaseUserContext.Instance.Commited += OnCommentSaved;
        }

        public void Delete()
        {
            BaseUserContext.Instance.Repository.SoftDelete<BusinessAccessLayer.Entities.Comment>(this.Id);
            this.IsDeleted = true;
            Notifiers.ToList().ForEach(el => el.TryNotify(this, CommentActionType.Delete));
        }

        public void AddFile(File file)
        {
            var filebox = GetOrCreateFileBox();
            FileBoxItem.CreateNew(filebox.Id, file);
        }

        public enum CommentOrder
        {
            OldestFirst = 0,
            RecentFirst = 1
        }

        public static IEnumerable<TComment> Get<TComment>(string businessKey, int? commentOrder, bool isEncrypted, bool isReadOnly, int? maxComments) where TComment : Comment, new()
        {
            Notifiers.ToList().ForEach(el => el.TryNotify(businessKey, CommentActionType.View));

            var query = BaseUserContext.Instance.Repository
                                           .GetQueryableSet<Entities.Comment, TComment>()
                                           .Where(el => el.BusinessKey == businessKey);

            if (commentOrder.HasValue)
            {
                if ((Comment.CommentOrder)commentOrder == Comment.CommentOrder.RecentFirst)
                    query = query.OrderByDescending(el => el.CreationDate);
                else
                    query = query.OrderBy(el => el.CreationDate);
            }
            else
                query = query.OrderBy(el => el.CreationDate);

            return Repository.Convert<Entities.Comment, TComment>(query).ToList();
        }
    }

    public class EncryptedComment : Comment
    {
        [EncryptedField]
        public override string Body
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
