using System.ComponentModel.DataAnnotations;

namespace PhotoSystem.Models
{
    public class StoryCreateViewModel
    {
        [Required(ErrorMessage = "Будь ласка, виберіть файл")]
        public IFormFile MediaFile { get; set; } // Сам файл (фото або відео)

        [MaxLength(150, ErrorMessage = "Підпис занадто довгий")]
        public string? Caption { get; set; } // Опціональний підпис
    }
}