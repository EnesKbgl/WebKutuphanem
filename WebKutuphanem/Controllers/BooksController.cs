using Microsoft.AspNetCore.Authorization; // <-- BU KÜTÜPHANE EKLENDİ (GÜVENLİK İÇİN)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKutuphanem.Data;
using WebKutuphanem.Models;

namespace WebKutuphanem.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;

        // Yapıcı Metot: Veritabanı bağlantısını (Context) içeri alır
        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LİSTELEME (HERKESE AÇIK)
        // ============================================================

        // GET: /Books/Index
        // Buraya [Authorize] KOYMADIK. Çünkü vitrini herkes görsün istiyoruz.
        public async Task<IActionResult> Index(string searchString, string statusFilter, string sortOrder)
        {
            // 1. Önce tüm kitapları sorgu olarak hazırla
            var books = from b in _context.Books select b;

            // 2. Arama Kelimesi varsa filtrele
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Title.Contains(searchString) || s.Author.Contains(searchString));
            }

            // 3. Durum Filtresi varsa uygula
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                books = books.Where(x => x.Status == statusFilter);
            }

            // 4. Sıralama
            switch (sortOrder)
            {
                case "title_asc":
                    books = books.OrderBy(b => b.Title);
                    break;
                case "author_asc":
                    books = books.OrderBy(b => b.Author);
                    break;
                case "progress_desc":
                    books = books.OrderByDescending(b => (double)b.CurrentPage / b.TotalPages);
                    break;
                default:
                    books = books.OrderByDescending(b => b.CreatedAt);
                    break;
            }

            // 5. Filtreleri hatırlamak için ViewData'ya at
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;
            ViewData["CurrentSort"] = sortOrder;

            // 6. Sonuçları getir
            return View(await books.ToListAsync());
        }

        // ============================================================
        // EKLEME İŞLEMLERİ (SADECE ÜYELER)
        // ============================================================

        [Authorize] // <-- YASAK GELDİ: Sadece giriş yapanlar görebilir
        public IActionResult Create()
        {
            return View();
        }

        [Authorize] // <-- YASAK GELDİ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // ============================================================
        // DÜZENLEME İŞLEMLERİ (SADECE ÜYELER)
        // ============================================================

        [Authorize] // <-- YASAK GELDİ
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        [Authorize] // <-- YASAK GELDİ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // ============================================================
        // SİLME İŞLEMLERİ (SADECE ÜYELER)
        // ============================================================

        [Authorize] // <-- YASAK GELDİ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}