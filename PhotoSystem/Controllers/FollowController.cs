using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoSystem.Data;
using PhotoSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSystem.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFollow(string targetUserId)
        {
            var currentUserId = _userManager.GetUserId(User);

            // не можна підписатися на самого себе
            if (currentUserId == targetUserId) return BadRequest("Ви не можете підписатися на себе.");

            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);

            if (existingFollow != null)
            {
                // Відписка
                _context.Follows.Remove(existingFollow);
            }
            else
            {
                // Підписка
                _context.Follows.Add(new Follow
                {
                    FollowerId = currentUserId!,
                    FollowingId = targetUserId
                });
            }

            await _context.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // Сторінка Підписники
        public async Task<IActionResult> Followers(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var followers = await _context.Follows
                .Where(f => f.FollowingId == userId)
                .Include(f => f.Follower)
                .Select(f => f.Follower)
                .ToListAsync();

            ViewBag.UserName = user?.Email;
            return View(followers);
        }

        // Сторінка Підписки
        public async Task<IActionResult> Following(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var following = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Include(f => f.Following)
                .Select(f => f.Following)
                .ToListAsync();

            ViewBag.UserName = user?.Email;
            return View(following);
        }
    }
}