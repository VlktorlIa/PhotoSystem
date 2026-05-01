using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoSystem.Data;
using PhotoSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSystem.Controllers
{
    public class FeedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedController(ApplicationDbContext context,
                              UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            //  Перевіряє, чи користувач увійшов у систему
            if (!User.Identity.IsAuthenticated)
            {
                return View(new List<PhotoPost>());
            }

            var currentUserId = _userManager.GetUserId(User);

            //  Отримує список ID тих, на кого підписаний користувач
            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            // Завантажує пости тільки цих користувачів
            var posts = await _context.PhotoPosts
                .Where(p => followingIds.Contains(p.UserId) && !p.IsDeleted) // Не показуємо видалені
                .Include(p => p.User)  // Щоб бачити автора
                .Include(p => p.Likes) // завантажуємо лайки для відображення у стрічці
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }
    }
}