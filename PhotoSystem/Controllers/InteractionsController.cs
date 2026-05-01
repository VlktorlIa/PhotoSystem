using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoSystem.Data;
using PhotoSystem.Models;

[Authorize]
public class InteractionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InteractionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Forward(int postId, string comment)
    {
        var original = await _context.PhotoPosts.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == postId);
        var userId = _userManager.GetUserId(User);

        if (original == null || original.UserId == userId) return BadRequest();

        // Створює новий пост-дублікат для стрічки користувача
        var newPost = new PhotoPost
        {
            UserId = userId,
            ImagePath = original.ImagePath,
            Description = comment ?? "Переслано",
            CreatedAt = DateTime.Now
        };
        _context.PhotoPosts.Add(newPost);
        await _context.SaveChangesAsync();

        // Фіксує зв'язок
        var interaction = new PostInteraction
        {
            Type = InteractionType.Forward,
            OriginalPostId = postId,
            OriginalAuthorId = original.UserId,
            ActorId = userId,
            Comment = comment,
            ReplyPostId = newPost.Id
        };
        _context.PostInteractions.Add(interaction);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Feed");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int postId, string comment)
    {
        // Отримує поточного користувача
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var originalPost = await _context.PhotoPosts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (originalPost == null) return NotFound();

        // Створює новий поствідповідь
        var replyPost = new PhotoPost
        {
            UserId = userId,
            ImagePath = originalPost.ImagePath, // Копіює фото для візуалізації відповіді
            Description = !string.IsNullOrEmpty(comment) ? comment : "Відповідь на публікацію",
            CreatedAt = DateTime.Now
        };

        _context.PhotoPosts.Add(replyPost);
        //просто SaveChangesAsync без MemoryCache
        await _context.SaveChangesAsync();

        // Фіксує взаємодію
        var interaction = new PostInteraction
        {
            OriginalPostId = postId,
            ReplyPostId = replyPost.Id,
            ActorId = userId, // використовуємо ActorId замість UserId
            OriginalAuthorId = originalPost.UserId,
            Type = InteractionType.Reply,
            Comment = comment,
            CreatedAt = DateTime.Now
        };

        _context.PostInteractions.Add(interaction);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Feed");
    }
}