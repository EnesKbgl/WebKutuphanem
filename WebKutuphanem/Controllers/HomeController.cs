using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Eklendi
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebKutuphanem.Data;
using WebKutuphanem.Models;

namespace WebKutuphanem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; // Eklendi

        // Constructor'a UserManager eklendi
        public HomeController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User); // Giriş yapan kişi

            // Tüm sorgulara .Where(b => b.UserId == userId) eklendi
            ViewBag.TotalBooks = _context.Books.Count(b => b.UserId == userId);
            ViewBag.ReadingCount = _context.Books.Count(b => b.Status == "reading" && b.UserId == userId);
            ViewBag.FinishedCount = _context.Books.Count(b => b.Status == "finished" && b.UserId == userId);

            var recentBooks = _context.Books
                .Where(b => b.UserId == userId) // Sadece benim kitaplarım
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToList();

            return View(recentBooks);
        }

        public IActionResult Statistics()
        {
            var userId = _userManager.GetUserId(User);

            // Sorgular filtrelendi
            ViewBag.BooksThisMonth = _context.Books
                .Count(b => b.UserId == userId && b.CreatedAt.Month == DateTime.Now.Month && b.CreatedAt.Year == DateTime.Now.Year);

            ViewBag.TotalReadPages = _context.Books
                .Where(b => b.UserId == userId)
                .Sum(b => (int?)b.CurrentPage) ?? 0;

            var userBooks = _context.Books.Where(b => b.UserId == userId);
            var totalBooks = userBooks.Count();

            ViewBag.AvgPageCount = totalBooks > 0
                ? (int)userBooks.Average(b => b.TotalPages)
                : 0;

            ViewBag.TotalBooks = totalBooks;

            return View();
        }

        [HttpGet]
        public IActionResult GetChartData()
        {
            var userId = _userManager.GetUserId(User);
            var myBooks = _context.Books.Where(b => b.UserId == userId); // Filtre

            var statusCounts = new
            {
                Reading = myBooks.Count(b => b.Status == "reading"),
                Finished = myBooks.Count(b => b.Status == "finished"),
                ToRead = myBooks.Count(b => b.Status == "to-read")
            };

            var topAuthors = myBooks
                .GroupBy(b => b.Author)
                .Select(g => new { Author = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var monthlyData = myBooks
                .AsEnumerable()
                .GroupBy(b => b.CreatedAt.ToString("MMM"))
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToList();

            return Json(new { statusCounts, topAuthors, monthlyData });
        }

        public IActionResult Privacy() { return View(); }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}