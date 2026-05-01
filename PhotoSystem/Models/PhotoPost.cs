using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PhotoSystem.Models
{
    public class PhotoPost
    {
        public int Id { get; set; }

        // Власник публікації
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        
        [Required]
        public string ImagePath { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // видалення
        public bool IsDeleted { get; set; } = false;

        public ICollection<PhotoLike> Likes { get; set; } = new List<PhotoLike>();
    }
}