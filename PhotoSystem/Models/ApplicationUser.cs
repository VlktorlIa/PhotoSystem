using Microsoft.AspNetCore.Identity;

namespace PhotoSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? AvatarPath { get; set; }
    }
}