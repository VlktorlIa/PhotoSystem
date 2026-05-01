using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PhotoSystem.Data;
using PhotoSystem.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class StoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _hostEnvironment;

    public StoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _hostEnvironment = hostEnvironment;
    }

    // Метод для отримання активних історій (для Story Bar)
    [HttpGet]
    public async Task<IActionResult> GetFeed()
    {
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == null) return Unauthorized();

        // Отримуєм список ID користувачів на яких підписаний поточний юзер
        var followingIds = await _context.Follows
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        // Додаєм свій ID до списку щоб бачити і власні історії
        followingIds.Add(currentUserId);

        // Вибирає історії лише цих людей
        var activeStories = await _context.Stories
            .Include(s => s.Author)
            .Include(s => s.Views)
            .Where(s => followingIds.Contains(s.AuthorId) // Тільки друзі + я
                        && s.ExpiresAt > DateTime.Now
                        && !s.IsArchived)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        // Групує для Story Bar
        var feed = activeStories
            .GroupBy(s => s.AuthorId)
            .Select(g => new {
                authorId = g.Key,
                authorName = g.First().Author.UserName,
                authorEmail = g.First().Author.Email,
                hasUnviewed = g.Any(s => !s.Views.Any(v => v.ViewerId == currentUserId)),
                storyCount = g.Count()
            })
            .OrderByDescending(x => x.hasUnviewed) // Нові історії спочатку
            .ToList();

        return Json(feed);
    }
    // GET: Stories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Stories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StoryCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);

            // Перевірка ліміту макс 10 активних історій
            var activeCount = await _context.Stories
                .CountAsync(s => s.AuthorId == user.Id && s.ExpiresAt > DateTime.Now);

            if (activeCount >= 10)
            {
                ModelState.AddModelError("", "Ви не можете мати більше 10 активних історій одночасно.");
                return View(model);
            }

            // Обробка файлу
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.MediaFile.FileName;
            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "stories");
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.MediaFile.CopyToAsync(fileStream);
            }

            // Визначає тип контенту
            var contentType = model.MediaFile.ContentType;
            MediaType type = contentType.Contains("video") ? MediaType.Video : MediaType.Photo;

            // Збереження в базу
            var story = new Story
            {
                MediaUrl = "/uploads/stories/" + uniqueFileName,
                Type = type,
                Caption = model.Caption,
                AuthorId = user.Id,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(24)
            };

            _context.Stories.Add(story);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Feed"); // Повертає у стрічку
        }
        return View(model);
    }

    // Отримати всі активні історії конкретного користувача
    [HttpGet("api/stories/user/{userId}")]
    public async Task<IActionResult> GetUserStories(string userId)
    {
        var stories = await _context.Stories
            .Include(s => s.Author)
            .Where(s => s.AuthorId == userId && s.ExpiresAt > DateTime.Now && !s.IsArchived)
            .OrderBy(s => s.CreatedAt)
            .Select(s => new {
                id = s.Id,
                mediaUrl = s.MediaUrl,
                type = (int)s.Type, // 0 - Photo, 1 - Video
                caption = s.Caption,
                authorName = s.Author.UserName,
                createdAt = s.CreatedAt
            })
            .ToListAsync();

        return Json(stories);
    }

    // Позначити історію як переглянуту
    [HttpPost("api/stories/{id}/view")]
    public async Task<IActionResult> MarkAsViewed(int id)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == null) return Unauthorized();

        // Перевіряє чи вже є такий перегляд, щоб не дублювати в базі
        var alreadyViewed = await _context.StoryViews
            .AnyAsync(v => v.StoryId == id && v.ViewerId == currentUserId);

        if (!alreadyViewed)
        {
            var view = new StoryView
            {
                StoryId = id,
                ViewerId = currentUserId,
                ViewedAt = DateTime.Now
            };
            _context.StoryViews.Add(view);
            await _context.SaveChangesAsync();
        }

        return Ok();
    }
}