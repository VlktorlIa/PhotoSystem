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

        public FollowController(ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Підписатися
        public async Task<IActionResult> FollowUser(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (currentUserId == id)
                return RedirectToAction("Profile", "Users", new { id });

            bool alreadyFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == currentUserId &&
                               f.FollowingId == id);

            if (!alreadyFollowing)
            {
                var follow = new Follow
                {
                    FollowerId = currentUserId,
                    FollowingId = id
                };

                _context.Follows.Add(follow);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Profile", "Users", new { id });
        }

        //  Відписатися
        public async Task<IActionResult> UnfollowUser(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId &&
                                          f.FollowingId == id);

            if (follow != null)
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Profile", "Users", new { id });
        }

        //  Followers list
        public async Task<IActionResult> Followers(string id)
        {
            var followers = await _context.Follows
                .Where(f => f.FollowingId == id)
                .Include(f => f.Follower)
                .ToListAsync();

            return View(followers);
        }

        // Following list
        public async Task<IActionResult> Following(string id)
        {
            var following = await _context.Follows
                .Where(f => f.FollowerId == id)
                .Include(f => f.Following)
                .ToListAsync();

            return View(following);
        }
    }
}