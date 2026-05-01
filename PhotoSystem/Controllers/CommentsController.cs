using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoSystem.Data;
using PhotoSystem.Models;
namespace PhotoSystem.Controllers

{

    [Authorize]

    public class CommentsController : Controller

    {

        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;



        public CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)

        {

            _context = context;

            _userManager = userManager;

        }



        // Створення коментаря (або відповіді)

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(int postId, string content, int? parentId, string returnUrl)

        {

            if (string.IsNullOrWhiteSpace(content)) return BadRequest();



            var comment = new Comment

            {

                PhotoPostId = postId,

                Content = content,

                AuthorId = _userManager.GetUserId(User),

                ParentId = parentId,

                CreatedAt = DateTime.Now

            };



            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();



            // Якщо ми передали returnUrl з якорем, повертаємо туди

            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);

            return Redirect(Request.Headers["Referer"].ToString());

        }



        // Видалення коментаря

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Delete(int id, string returnUrl)

        {

            var comment = await _context.Comments.FindAsync(id);

            var currentUserId = _userManager.GetUserId(User);



            if (comment == null || comment.AuthorId != currentUserId)

                return Forbid();



            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();



            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);

            return Redirect(Request.Headers["Referer"].ToString());

        }



        // Редагування коментаря

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, string content, string returnUrl)

        {

            var comment = await _context.Comments.FindAsync(id);

            var currentUserId = _userManager.GetUserId(User);



            if (comment == null || comment.AuthorId != currentUserId)

            {

                return Forbid();

            }



            if (string.IsNullOrWhiteSpace(content))

            {

                return BadRequest();

            }



            comment.Content = content;

            comment.UpdatedAt = DateTime.Now;



            _context.Update(comment);

            await _context.SaveChangesAsync();



            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);

            return Redirect(Request.Headers["Referer"].ToString());

        }

    }

}