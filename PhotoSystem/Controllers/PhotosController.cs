using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoSystem.Data;
using PhotoSystem.Models;

namespace PhotoSystem.Controllers
{
    public class PhotosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public PhotosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        //  Мої публікації (тільки для авторизованих)
        [Authorize]
        public async Task<IActionResult> MyPosts()
        {
            var userId = _userManager.GetUserId(User);

            var posts = await _context.PhotoPosts
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        //  Перегляд публікацій іншого користувача (доступно всім)
        public async Task<IActionResult> UserPosts(string id)
        {
            var posts = await _context.PhotoPosts
                .Include(p => p.User)
                .Where(p => p.UserId == id && !p.IsDeleted)
                .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.UserId = id;
            return View(posts);
        }

        //  GET - створення поста
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        //  POST — створення поста (завантаження файлу)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile image, string? description)
        {
            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError("", "Оберіть файл зображення.");
                return View();
            }

            // простий контроль типу
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError("", "Дозволені формати: jpg, jpeg, png, gif, webp.");
                return View();
            }

            // унікальне ім'я
            var fileName = $"{Guid.NewGuid()}{ext}";

            // шлях до wwwroot/uploads
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadDir);

            var fullPath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var post = new PhotoPost
            {
                UserId = _userManager.GetUserId(User),
                ImagePath = "/uploads/" + fileName,
                Description = description,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.PhotoPosts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
        }

        // Видалити свою публікацію (soft delete)
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.PhotoPosts.FindAsync(id);
            if (post == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            // не можна видаляти чужі
            if (post.UserId != userId) return Forbid();

            post.IsDeleted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
        }

        //  Сторінка “Видалені” (для відновлення)
        [Authorize]
        public async Task<IActionResult> Deleted()
        {
            var userId = _userManager.GetUserId(User);

            var posts = await _context.PhotoPosts
                .Where(p => p.UserId == userId && p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        //  Відновити
        [Authorize]
        public async Task<IActionResult> Restore(int id)
        {
            var post = await _context.PhotoPosts.FindAsync(id);
            if (post == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (post.UserId != userId) return Forbid();

            post.IsDeleted = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPosts));
        }

        
        //  LAB 10: ЛАЙКИ (5)
       

        //  Поставити лайк (з емоджі)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id, string emoji = "❤️")
        {
            var userId = _userManager.GetUserId(User);

            var post = await _context.PhotoPosts
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (post == null) return NotFound();

            // 1 користувач = 1 лайк на 1 фото
            var already = await _context.PhotoLikes
                .AnyAsync(l => l.PhotoPostId == id && l.UserId == userId);

            if (!already)
            {
                _context.PhotoLikes.Add(new PhotoLike
                {
                    PhotoPostId = id,
                    UserId = userId!,
                    Emoji = string.IsNullOrWhiteSpace(emoji) ? "❤️" : emoji,
                    LikedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            // повертаємось назад на ту сторінку, де натиснули
            return Redirect(Request.Headers["Referer"].ToString());
        }

        //  Прибрати тільки свій лайк
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlike(int id)
        {
            var userId = _userManager.GetUserId(User);

            var like = await _context.PhotoLikes
                .FirstOrDefaultAsync(l => l.PhotoPostId == id && l.UserId == userId);

            if (like != null)
            {
                _context.PhotoLikes.Remove(like);
                await _context.SaveChangesAsync();
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
