using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PhotoSystem.Models
{
    public enum MediaType { Photo, Video }

    public class Story
    {
        public int Id { get; set; }

        [Required]
        public string MediaUrl { get; set; } // Шлях до файлу 

        public MediaType Type { get; set; }

        [MaxLength(150)]
        public string? Caption { get; set; } // Підпис

        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddHours(24);

        public bool IsArchived { get; set; } = false;

        // Зв'язок з переглядами
        public virtual ICollection<StoryView> Views { get; set; } = new List<StoryView>();
    }
}