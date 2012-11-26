using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    /// <summary>
    /// Represents a user comment.
    /// </summary>
    public class Comment : BusinessEntity
    {
        public virtual int Id { get; set; }
        [MaxLength(250)]
        public virtual string BusinessKey { get; set; }
        public virtual string Body { get; set; }
        public virtual int? CommenterId { get; set; }
        public virtual DateTime CreationDate { get; set; }

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

        public static void Create(string businessKey, string comment, int? commenterId = null)
        {
            var newComment = new Comment
                                 {
                                     BusinessKey = businessKey,
                                     Body = comment.Trim(),
                                     CommenterId = commenterId
                                 };

            BaseUserContext.Instance.Repository.Add<Entities.Comment, Comment>(newComment);
        }
    }
}
