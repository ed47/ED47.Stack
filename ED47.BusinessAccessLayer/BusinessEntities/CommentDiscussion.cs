using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class CommentDiscussion : BusinessEntity //Name doens't matches entity because EF 5.0 is silly and can't distringuish types by namespaces.
    {
        [MaxLength(250)]
        public virtual string BusinessKey { get; set; }
        public virtual bool IsEncrypted { get; set; }
        public virtual int? CommentOrder { get; set; }
        public virtual int? MaxComments { get; set; }
        public virtual bool IsReadOnly { get; set; }

        public static TCommentDiscussion GetOrCreate<TCommentDiscussion>(string businessKey, bool isEncrypted = false, CommentOrder commentOrder = BusinessEntities.CommentOrder.RecentFirst, int? maxComments = null, bool isReadOnly = false) where TCommentDiscussion : CommentDiscussion, new()
        {
            var discussion = BaseUserContext.Instance.Repository.Find<Entities.Discussion, TCommentDiscussion>(el => el.BusinessKey == businessKey);

            if (discussion != null)
                return discussion;

            return Create<TCommentDiscussion>(businessKey, isEncrypted, commentOrder, maxComments, isReadOnly);
        }

        public static TCommentDiscussion Create<TCommentDiscussion>(string businessKey, bool isEncrypted = false, CommentOrder commentOrder = BusinessEntities.CommentOrder.RecentFirst, int? maxComments = null, bool isReadOnly = false) where TCommentDiscussion : CommentDiscussion, new()
        {
            var newDiscussion = new TCommentDiscussion
                {
                    BusinessKey  = businessKey,
                    CommentOrder = (int)commentOrder,
                    IsEncrypted = isEncrypted,
                    IsReadOnly = isReadOnly,
                    MaxComments = maxComments
                };

            BaseUserContext.Instance.Repository.Add<Entities.Discussion, TCommentDiscussion>(newDiscussion);

            return newDiscussion;
        }

        private IEnumerable<Comment> _comments;
        public virtual IEnumerable<Comment> Comments
        {
            get
            {
                return _comments ?? (_comments = Comment.Get<Comment>(BusinessKey, CommentOrder, IsEncrypted, IsReadOnly, MaxComments));
            }
        }
    }
}
