using Microsoft.AspNetCore.Mvc;
using WebKutuphanem.Data;
using WebKutuphanem.Models;

namespace WebKutuphanem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // Ana Sayfa (Dashboard)
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
        // İstatistikler Sayfası
        public IActionResult Statistics()
        {
            // Kart 1: Bu Ay Eklenen Kitap Sayısı
            ViewBag.BooksThisMonth = _context.Books
                .Count(b => b.CreatedAt.Month == DateTime.Now.Month && b.CreatedAt.Year == DateTime.Now.Year);

            // Kart 2: Toplam Okunan Sayfa (Tüm kitaplardaki 'Mevcut Sayfa'ların toplamı)
            // Eğer veritabanı boşsa hata vermesin diye (int?) kontrolü yapıyoruz
            ViewBag.TotalReadPages = _context.Books.Sum(b => (int?)b.CurrentPage) ?? 0;

            // Kart 3: Ortalama Kitap Kalınlığı (Toplam Sayfa / Kitap Sayısı)
            var totalBooks = _context.Books.Count();
            ViewBag.AvgPageCount = totalBooks > 0
                ? (int)_context.Books.Average(b => b.TotalPages)
                : 0;

            // Kart 4: En Yüksek İlerleme (En çok okuduğun kitabın yüzdesi)
            // Basitlik olsun diye şimdilik "Toplam Kitap" gösterelim
            ViewBag.TotalBooks = totalBooks;

            return View();
        }

        // --- GRAFİK VERİLERİNİ HAZIRLAYAN METOT ---
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

            // 3. Çizgi Grafik: Aylık Veriler (Basit örnek)
            var monthlyData = _context.Books
                .AsEnumerable()
                .GroupBy(b => b.CreatedAt.ToString("MMM"))
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToList();

            return Json(new { statusCounts, topAuthors, monthlyData });
        }

        // Hata sayfası vs.
        public IActionResult Privacy() { return View(); }
    }
}