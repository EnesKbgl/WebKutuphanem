using Microsoft.AspNetCore.Authorization; // <-- KİLİT İÇİN GEREKLİ KÜTÜPHANE
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics; // <-- Hata sayfası için gerekli
using WebKutuphanem.Data;
using WebKutuphanem.Models;

namespace WebKutuphanem.Controllers
{
    // SINIFIN TEPESİNE BUNU EKLEDİK:
    // Artık giriş yapmayan hiç kimse bu sayfadaki (Index, Statistics vb.) verilere erişemez.
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // Ana Sayfa (Dashboard)
        // [Authorize] sınıfın tepesinde olduğu için burası artık korumalıdır.
        public IActionResult Index()
        {
            // Kartlardaki Sayılar
            ViewBag.TotalBooks = _context.Books.Count();
            ViewBag.ReadingCount = _context.Books.Count(b => b.Status == "reading");
            ViewBag.FinishedCount = _context.Books.Count(b => b.Status == "finished");

            // Son Eklenen 5 Kitap
            var recentBooks = _context.Books
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToList();

            return View(recentBooks);
        }

        // İstatistikler Sayfası
        public IActionResult Statistics()
        {
            // Kart 1: Bu Ay Eklenen Kitap Sayısı
            ViewBag.BooksThisMonth = _context.Books
                .Count(b => b.CreatedAt.Month == DateTime.Now.Month && b.CreatedAt.Year == DateTime.Now.Year);

            // Kart 2: Toplam Okunan Sayfa
            ViewBag.TotalReadPages = _context.Books.Sum(b => (int?)b.CurrentPage) ?? 0;

            // Kart 3: Ortalama Kitap Kalınlığı
            var totalBooks = _context.Books.Count();
            ViewBag.AvgPageCount = totalBooks > 0
                ? (int)_context.Books.Average(b => b.TotalPages)
                : 0;

            // Kart 4: Toplam Kitap (Basitlik için)
            ViewBag.TotalBooks = totalBooks;

            return View();
        }

        // --- GRAFİK VERİLERİNİ HAZIRLAYAN METOT ---
        // Bu metot da [Authorize] sayesinde korunur.
        // Giriş yapmayan biri dışarıdan veri çekemez.
        [HttpGet]
        public IActionResult GetChartData()
        {
            // 1. Pasta Grafik: Kitap Durumları
            var statusCounts = new
            {
                Reading = _context.Books.Count(b => b.Status == "reading"),
                Finished = _context.Books.Count(b => b.Status == "finished"),
                ToRead = _context.Books.Count(b => b.Status == "to-read")
            };

            // 2. Bar Grafik: En Çok Okunan Yazarlar (İlk 5)
            var topAuthors = _context.Books
                .GroupBy(b => b.Author)
                .Select(g => new { Author = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // 3. Çizgi Grafik: Aylık Veriler
            var monthlyData = _context.Books
                .AsEnumerable()
                .GroupBy(b => b.CreatedAt.ToString("MMM"))
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToList();

            return Json(new { statusCounts, topAuthors, monthlyData });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // --- HATA SAYFASI (ÖNEMLİ) ---
        // Eğer giriş yapmamış biri hata alırsa sonsuz döngüye girmesin diye
        // burayı [AllowAnonymous] ile herkese açıyoruz.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}