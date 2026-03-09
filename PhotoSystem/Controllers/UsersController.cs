using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoSystem.Models;
using PhotoSystem.Data;

namespace PhotoSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Список користувачів
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // Профіль користувача
        public async Task<IActionResult> Profile(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // кількість підписників
            var followersCount = await _context.Follows
                .CountAsync(f => f.FollowingId == id);

            // кількість підписок
            var followingCount = await _context.Follows
                .CountAsync(f => f.FollowerId == id);

            ViewBag.FollowersCount = followersCount;
            ViewBag.FollowingCount = followingCount;

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                var isFollowing = await _context.Follows
                    .AnyAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == id);

                ViewBag.IsFollowing = isFollowing;
            }

            return View(user);
        }
    }
}
