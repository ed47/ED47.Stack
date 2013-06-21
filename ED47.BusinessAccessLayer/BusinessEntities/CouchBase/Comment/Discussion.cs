﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ED47.BusinessAccessLayer.Couchbase;
using Newtonsoft.Json;

namespace ED47.BusinessAccessLayer.BusinessEntities.CouchBase.Comment
{
    public class Discussion : BaseDocument,IDiscussion
    {
        [MaxLength(250)]
        public string BusinessKey { get; set; }
        public string Body { get; set; }
        public string Creator { get; set; }
        private IFileBox _fileBox;
        public IFileBox FileBox
        {
            get { return _fileBox ?? (_fileBox = CouchBase.Comment.FileBox.CreateNew("discussion")); }
            set { _fileBox = value; }
        }
        public DateTime? DeletionDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsEncrypted { get; set; }
        public CommentOrder CommentOrder { get; set; }
        public HashSet<string> Notifiers { get; set; }

        [JsonProperty]
        List<Comment> Comments = new List<Comment>();

        [JsonIgnore]
        public IEnumerable<IComment> Replies
        {
            get { return Comments; }
        }

        public Dictionary<string,IComment> AllComments = new Dictionary<string, IComment>();

        internal static string CalculateKey(string businessKey)
        {
            return ("discussion?key=" + businessKey).ToLower();
        }

        public override string GetKey()
        {
            return CalculateKey(BusinessKey).ToLower();
        }

        public override void Init()
        {
            base.Init();
            InitAllComments(this);
        }

        private void InitAllComments(IComment comment)
        {
            AllComments.Add(comment.BusinessKey, comment);
            if (!comment.Replies.Any()) return;
            foreach (var com  in comment.Replies)
            {
                InitAllComments(com);
            }
        }


        public static TDiscussion GetOrCreate<TDiscussion>(string businessKey, bool isEncrypted = false, CommentOrder commentOrder = CommentOrder.RecentFirst, int? maxComments = null, bool isReadOnly = false) where TDiscussion : class, IDiscussion, IDocument, new()
        {
            var discussion = CouchbaseRepository.Get<TDiscussion>(new {BusinessKey = businessKey});
            return discussion ?? Create<TDiscussion>(businessKey, isEncrypted, commentOrder, maxComments, isReadOnly);
        }

        public static TDiscussion Create<TDiscussion>(string businessKey, bool isEncrypted = false, CommentOrder commentOrder = CommentOrder.RecentFirst, int? maxComments = null, bool isReadOnly = false) where TDiscussion : class, IDiscussion, IDocument, new()
        {
            var newDiscussion = new TDiscussion
            {
                BusinessKey = businessKey,
                Key = CalculateKey(businessKey),
                CommentOrder = commentOrder,
                IsEncrypted = isEncrypted,
                Notifiers = new HashSet<string>()
            };

            newDiscussion.Save();

            return newDiscussion;
        }

        public IComment Reply(string body, string creator = null, bool? encrypted = false)
        {
            if (!CanReply()) return null;
            IsReadOnly = true;
            var newComment = Comment.Create(body, creator, encrypted);
            Comments.Add((Comment) newComment);
            return newComment;
        }

        public IComment Reply(string businesskey, string body, string creator = null, bool? encrypted = false)
        {
            if (!AllComments.ContainsKey(businesskey))
                return null;
            var comment = AllComments[businesskey];
            return comment.Reply(body, creator, encrypted);
        }

        public void AddFile(IFile file)
        {
            FileBox.AddFile(file);
        }

        public bool CanWrite()
        {
            return !IsReadOnly && !IsDeleted;
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

        public new bool Delete()
        {
            if (CanDelete())
            {
                IsDeleted = true;
                return true;
            }

            return false;
        }
    }
}
