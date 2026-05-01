using System.ComponentModel.DataAnnotations;

namespace PhotoSystem.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Коментар не може бути порожнім")]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; }

        // Зв'язок з постом
        public int PhotoPostId { get; set; }
        public PhotoPost PhotoPost { get; set; }

        // Зв'язок з автором
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

        // Для вкладених відповідей
        public int? ParentId { get; set; }
        public Comment Parent { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}