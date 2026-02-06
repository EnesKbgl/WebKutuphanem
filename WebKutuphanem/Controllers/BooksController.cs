using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // UserManager için gerekli
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKutuphanem.Data;
using WebKutuphanem.Models;

namespace WebKutuphanem.Controllers
{
    [Authorize] // Sadece üyeler girebilir
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; // Kullanıcıyı bulmak için

        // Yapıcı Metoda UserManager ekledik
        public BooksController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Books/Index
        public async Task<IActionResult> Index(string searchString, string statusFilter, string sortOrder)
        {
            // 1. Önce GİRİŞ YAPAN KULLANICIYI bul
            var userId = _userManager.GetUserId(User);

            // 2. Sadece BU KULLANICIYA ait kitapları çek (Filtreleme burada başlıyor)
            var books = from b in _context.Books
                        where b.UserId == userId
                        select b;

            // --- Arama, Filtreleme ve Sıralama Kodların Aynen Kalıyor ---
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Title.Contains(searchString) || s.Author.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                books = books.Where(x => x.Status == statusFilter);
            }

            switch (sortOrder)
            {
                case "title_asc": books = books.OrderBy(b => b.Title); break;
                case "author_asc": books = books.OrderBy(b => b.Author); break;
                case "progress_desc": books = books.OrderByDescending(b => (double)b.CurrentPage / b.TotalPages); break;
                default: books = books.OrderByDescending(b => b.CreatedAt); break;
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;
            ViewData["CurrentSort"] = sortOrder;

            return View(await books.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            // Validasyon sırasında UserId'yi görmezden gel (biz arka planda atayacağız)
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                // KİTABI KAYDETMEDEN ÖNCE SAHİBİNİ BELİRLE
                book.UserId = _userManager.GetUserId(User);

                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Sadece BENİM kitabımı getir (Başkası URL'den id değiştirip erişemesin)
            var userId = _userManager.GetUserId(User);
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (book == null) return NotFound(); // Kitap yoksa veya başkasınınsa hata ver

            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id) return NotFound();

            // Güvenlik: Kullanıcı başkasının kitabını ID değiştirip update edemez
            var userId = _userManager.GetUserId(User);
            var existingBook = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (existingBook == null) return Unauthorized(); // Başkasının kitabına dokunma!

            // Formdan UserId gelmez, onu biz tekrar atayalım ki silinmesin
            book.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Silerken de kontrol et: Bu kitap benim mi?
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}