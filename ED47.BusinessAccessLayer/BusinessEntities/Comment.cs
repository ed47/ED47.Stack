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
        Edit
    }

    /// <summary>
    /// Represents a user comment.
    /// </summary>
    public class Comment : BusinessEntity
    {

        private static readonly List<CommentNotifier> _Notifiers = new List<CommentNotifier>();

        public static void AddNotifier(CommentNotifier notifier)
        {
            _Notifiers.Add(notifier);
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

        /// <summary>
        /// Returns the comments by their business key.
        /// </summary>
        /// <param name="businessKey">The business key to get comments for.</param>
        public static IEnumerable<Comment> GetByBusinessKey(string businessKey)
        {
            return BaseUserContext.Instance.Repository
                    .Where<Entities.Comment, Comment>(el => el.BusinessKey == businessKey)
                    .OrderByDescending(el => el.CreationDate)
                    .ToList();
        }

        public static Comment Create(string businessKey, string comment, int? commenterId = null, IEnumerable<int> fileIds = null)
        {
            var newComment = new Comment
                                 {
                                     BusinessKey = businessKey,
                                     Body = comment.Trim(),
                                     CommenterId = commenterId
                                 };

            BaseUserContext.Instance.Repository.Add<Entities.Comment, Comment>(newComment);

            newComment.AddFiles(fileIds);
            _Notifiers.ForEach(el => el.TryNotify(newComment, CommentActionType.New));
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
            _Notifiers.ForEach(el => el.TryNotify(this, CommentActionType.Edit));
        }
            

        public void Save()
        {
            BaseUserContext.Instance.Repository.Update<Entities.Comment, Comment>(this);
            BaseUserContext.Instance.Commited += OnCommentSaved;
        }

        public void Delete()
        {
            BaseUserContext.Instance.Repository.SoftDelete<BusinessAccessLayer.Entities.Comment>(this.Id);
            this.IsDeleted = true;
            _Notifiers.ForEach(el => el.TryNotify(this, CommentActionType.Delete));
        }

        public void AddFile(File file)
        {
            var filebox = GetOrCreateFileBox();
            FileBoxItem.CreateNew(filebox.Id, file);
        }
    }
}
