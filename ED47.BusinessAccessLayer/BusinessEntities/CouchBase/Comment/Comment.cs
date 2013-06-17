using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using ED47.BusinessAccessLayer.Couchbase;

namespace ED47.BusinessAccessLayer.BusinessEntities.CouchBase.Comment
{
    public class Comment : BaseDocument,IComment
    {
        protected static readonly ICollection<CommentNotifier> Notifiers = new List<CommentNotifier>();

        public static void AddNotifier(CommentNotifier notifier)
        {
            Notifiers.Add(notifier);
        }

        [MaxLength(250)]
        public string BusinessKey { get; set; }
        public string Body { get; set; }
        public int? CommenterId { get; set; }
        public int? FileBoxId { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletionDate { get; set; }
        public string CommentType { get; set; }

        /// <summary>
        /// Returns the comments by their business key.
        /// </summary>
        /// <param name="businessKey">The business key to get comments for.</param>
        [Obsolete("User Get() instead.")]
        public static IEnumerable<IComment> GetByBusinessKey(string businessKey)
        {
            Notifiers.ToList().ForEach(el => el.TryNotify(businessKey, CommentActionType.View));

            return BaseUserContext.Instance.Repository
                    .Where<Entities.Comment, Comment>(el => el.BusinessKey == businessKey)
                    .OrderByDescending(el => el.CreationDate)
                    .ToList();
        }

        public static IComment Create(string businessKey, string comment, int? commenterId = null, IEnumerable<int> fileIds = null, bool? encrypted = false)
        {
            Comment newComment;

            if(encrypted == null || !encrypted.Value)
                newComment = new Comment();
            else
                newComment = new EncryptedComment();

            newComment.BusinessKey = businessKey;
            newComment.Body = comment.Trim();
            newComment.CommenterId = commenterId;
            newComment.Save();
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

                if (file != null)
                {
                    FileBoxItem.CreateNew(filebox.Id, file);
                }
            }
        }

        /// <summary>
        /// Add files from an existing list of FileBoxItems to the comment.
        /// </summary>
        /// <param name="files">The FileBoxItems to add to the comment.</param>
        public void AddFiles(IEnumerable<IFileBoxItem> files)
        {
            var filebox = GetOrCreateFileBox();

            foreach (var fileBoxItem in files)
            {
                FileBoxItem.CreateNew(filebox.Id, fileBoxItem.File);
            }
        }

        private IFileBox GetOrCreateFileBox()
        {
            IFileBox filebox;

            if (FileBoxId == null)
            {
                filebox = CouchBase.Comment.FileBox.CreateNew("Comment");
                FileBoxId = filebox.Id;
                Save();
            }
            else
                filebox = FileBox;
            return filebox;
        }

        private IFileBox _fileBox;
        public IFileBox FileBox
        {
            get { return _fileBox ?? (_fileBox = FileBoxId.HasValue ? CouchBase.Comment.FileBox.Get(FileBoxId.Value) : null); }
        }


        //public void OnCommentSaved(object sender, EventArgs e)
        //{
        //    BaseUserContext.Instance.Commited -= OnCommentSaved;
        //    Notifiers.ToList().ForEach(el => el.TryNotify(this, CommentActionType.Edit));
        //}
            

        public new void Save()
        {
            //BaseUserContext.Instance.Repository.Update<Entities.Comment, Comment>(this);
            base.Save();
            //BaseUserContext.Instance.Commited += OnCommentSaved;
            Notifiers.ToList().ForEach(el => el.TryNotify(this, CommentActionType.Edit));
        }

        public new void Delete()
        {
            //BaseUserContext.Instance.Repository.SoftDelete<BusinessAccessLayer.Entities.Comment>(this.Id);
            base.Delete();
            IsDeleted = true;
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
                query = (CommentOrder)commentOrder == CommentOrder.RecentFirst ? query.OrderByDescending(el => el.CreationDate) : query.OrderBy(el => el.CreationDate);
            }
            else
                query = query.OrderBy(el => el.CreationDate);

            if (maxComments.HasValue)
                query = query.Take(maxComments.Value);

            return Repository.Convert<Entities.Comment, TComment>(query).ToList();
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
