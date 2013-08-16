using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
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

        public DateTime? ModificationDate { get; set; }

        public bool IsDeleted { get; set; }

        public string Title { get; set; }

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

        [JsonIgnore]
        public Dictionary<string,IComment> AllComments = new Dictionary<string, IComment>();

        public static string CalculateKey(string businessKey)
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


        public static TDiscussion GetOrCreate<TDiscussion>(string businessKey,string title=null, string body=null,string creator=null, bool isEncrypted = false, CommentOrder commentOrder = CommentOrder.RecentFirst, int? maxComments = null, bool isReadOnly = false) where TDiscussion : class, IDiscussion, IDocument, new()
        {
            var discussion = Get<TDiscussion>(businessKey);
            return discussion ?? Create<TDiscussion>(businessKey, title,body,creator, isEncrypted, commentOrder, maxComments, isReadOnly);
        }

        public static TDiscussion Get<TDiscussion>(string businessKey) where TDiscussion : class, IDiscussion, IDocument, new()
        {
            return CouchbaseRepository.Get<TDiscussion>(new {BusinessKey = businessKey});
        }

        public static TDiscussion Create<TDiscussion>(string businessKey,string title=null, string body=null,string creator=null, bool isEncrypted = false, CommentOrder commentOrder = CommentOrder.RecentFirst, int? maxComments = null, bool isReadOnly = false) where TDiscussion : class, IDiscussion, IDocument, new()
        {
            var newDiscussion = new TDiscussion
            {
                BusinessKey = businessKey,
                Key = CalculateKey(businessKey),
                CommentOrder = commentOrder,
                IsEncrypted = isEncrypted,
                Title = title,
                Body = body,
                Creator = creator,
                Notifiers = new HashSet<string>()
            };
            newDiscussion.Notifiers.Add(creator);

            newDiscussion.Save();

            return newDiscussion;
        }

        public IComment Reply(string body, string creator = null, bool? encrypted = false)
        {
            if (!CanReply()) return null;
            var newComment = Comment.Create(body, creator, encrypted);
            Comments.Add((Comment) newComment);
            Notifiers.Add(creator);
            return newComment;
        }

        public IComment Reply(string businessKey, string body, string creator = null, bool? encrypted = false)
        {
            if (!AllComments.ContainsKey(businessKey))
                return null;
            var comment = AllComments[businessKey];
            Notifiers.Add(creator);
            return comment.Reply(body, creator, encrypted);
        }

        public void AddFile(IFile file)
        {
            FileBox.AddFile(file,BusinessKey);
        }

        public void AddFile(string businessKey, IFile file )
        {
            if (!AllComments.ContainsKey(businessKey))
                return ;
            var comment = AllComments[businessKey];
            comment.AddFile(file);
        }


        public bool DeleteFile(string businessKey, string fileId)
        {
            if (!AllComments.ContainsKey(businessKey))
                return false;
            var comment = AllComments[businessKey];
            return comment.FileBox.DeleteFile(fileId);
        }


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

        public bool Edit(string businesskey, string body)
        {
            if (!AllComments.ContainsKey(businesskey))
                return false;
            var comment = AllComments[businesskey];
            return comment.Edit(body);
        }

        public bool Delete(string businesskey)
        {
            if (!AllComments.ContainsKey(businesskey))
                return false;
            var comment = AllComments[businesskey];
            return comment.Delete();
        }

        public bool CanWrite()
        {
            return Replies.All(el => el.IsDeleted) && !IsDeleted;
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
