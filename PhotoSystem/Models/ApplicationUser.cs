using Microsoft.AspNetCore.Identity;

namespace PhotoSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? AvatarPath { get; set; }
        public virtual ICollection<Follow> Followers { get; set; }
        public virtual ICollection<Follow> Following { get; set; }
    }
}