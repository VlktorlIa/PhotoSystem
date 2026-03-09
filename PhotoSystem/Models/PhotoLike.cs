using System;
using System.ComponentModel.DataAnnotations;

namespace PhotoSystem.Models
{
    public class PhotoLike
    {
        public int Id { get; set; }

        [Required]
        public int PhotoPostId { get; set; }
        public PhotoPost? PhotoPost { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        public ApplicationUser? User { get; set; }

        // Додатково: емоджі
        [Required]
        public string Emoji { get; set; } = "❤️";

        public DateTime LikedAt { get; set; } = DateTime.Now;
    }
}
