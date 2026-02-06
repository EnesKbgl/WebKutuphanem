using System.ComponentModel.DataAnnotations;

namespace WebKutuphanem.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kitap adı zorunludur.")]
        public string Title { get; set; } // Kitap Başlığı

        [Required(ErrorMessage = "Yazar adı zorunludur.")]
        public string Author { get; set; } // Yazar

        public string? ISBN { get; set; } // ISBN (Boş olabilir)

        public string? CoverImage { get; set; } // Kapak Resmi URL (Boş olabilir)

        [Required]
        public string Status { get; set; } // Durum (Okunacak, Okuyor, Bitti)

        [Required]
        public int TotalPages { get; set; } // Toplam Sayfa

        public int CurrentPage { get; set; } = 0; // Mevcut Sayfa (Varsayılan 0)

        // Kayıt Tarihi (Otomatik atansın diye)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}