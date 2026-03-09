using System;
using System.ComponentModel.DataAnnotations;

namespace PhotoSystem.Models
{
    public class Follow
    {
        public int Id { get; set; }

        [Required]
        public string FollowerId { get; set; }
        public ApplicationUser Follower { get; set; }

        [Required]
        public string FollowingId { get; set; }
        public ApplicationUser Following { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}