using System;

namespace PhotoSystem.Models
{
    public class StoryView
    {
        public int Id { get; set; }

        public int StoryId { get; set; }
        public virtual Story Story { get; set; }

        public string ViewerId { get; set; }
        public virtual ApplicationUser Viewer { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.Now;
    }
}