using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoSystem.Models
{
    public enum InteractionType { Reply, Forward }

    public class PostInteraction
    {
        [Key]
        public int Id { get; set; }

        public InteractionType Type { get; set; } // Reply або Forward

        // Посилання на оригінальний пост
        public int OriginalPostId { get; set; }
        [ForeignKey("OriginalPostId")]
        public virtual PhotoPost OriginalPost { get; set; }

        // Хто створив оригінал
        public string OriginalAuthorId { get; set; }
        public virtual ApplicationUser OriginalAuthor { get; set; }

        // Хто пересилає відповідає зараз
        public string ActorId { get; set; }
        public virtual ApplicationUser Actor { get; set; }

        public string? Comment { get; set; }

        // Посилання на новий пост (який з'явиться у стрічці актора)
        public int? ReplyPostId { get; set; }
        [ForeignKey("ReplyPostId")]
        public virtual PhotoPost? ReplyPost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}