using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public static class CommentRepositoryExtension
    {
        /// <summary>
        /// Makes the previous comment read-only.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="currentCommentId">The current comment.</param>
        /// <param name="businessKey">The comment's business key.</param>
        public static void MakePreviousCommentReadOnly(this IRepository repository, int currentCommentId, string businessKey)
        {
            var comments = repository
                                .GetQueryableSet<Entities.Comment, Comment>()
                                .Where(el => el.BusinessKey == businessKey && el.Id != currentCommentId)
                                .OrderByDescending(el => el.CreationDate);
                
            var previousComment = comments.FirstOrDefault();

            if (previousComment == null)
                return;

            previousComment.IsReadOnly = true;
        }
    }
}
