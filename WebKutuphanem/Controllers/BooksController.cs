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

        // GET: /Books/Index
        // Kitapları veritabanından çeker ve sayfaya gönderir
        // GET: /Books/Index
        // Arama (searchString), Filtreleme (statusFilter) ve Sıralama (sortOrder) parametrelerini alır
        public async Task<IActionResult> Index(string searchString, string statusFilter, string sortOrder)
        {
            // 1. Önce tüm kitapları sorgu olarak hazırla (Henüz veritabanına gitmedi)
            var books = from b in _context.Books
                        select b;

            // 2. Arama Kelimesi varsa filtrele (Başlık veya Yazarda ara)
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Title.Contains(searchString) || s.Author.Contains(searchString));
            }

            // 3. Durum Filtresi varsa (Okunacak, Okuyor vb.) uygula
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                books = books.Where(x => x.Status == statusFilter);
            }

            // 4. Sıralama (Switch-Case ile karar ver)
            switch (sortOrder)
            {
                case "title_asc":
                    books = books.OrderBy(b => b.Title); // A-Z
                    break;
                case "author_asc":
                    books = books.OrderBy(b => b.Author); // Yazar A-Z
                    break;
                case "progress_desc":
                    // İlerlemeye göre (Yüzde hesaplayıp sıralar)
                    books = books.OrderByDescending(b => (double)b.CurrentPage / b.TotalPages);
                    break;
                default:
                    books = books.OrderByDescending(b => b.CreatedAt); // Varsayılan: En Yeni
                    break;
            }

            // 5. Seçilen filtreleri sayfada tekrar göstermek için geri gönder (ViewData)
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;
            ViewData["CurrentSort"] = sortOrder;

            // 6. Sonuçları getir
            return View(await books.ToListAsync());
        }

        // GET: /Books/Create
        // Boş form sayfasını açar
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Books/Create
        // Formdan gelen verileri (book) alır ve veritabanına kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                // 1. Kitabı veritabanı sırasına ekle
                _context.Add(book);
                // 2. "Kaydet" butonuna bas (SQL'e yaz)
                await _context.SaveChangesAsync();
                // 3. İş bitince listeye geri dön
                return RedirectToAction(nameof(Index));
            }
            // Hata varsa formu tekrar göster (Hata mesajlarıyla birlikte)
            return View(book);

        }
        // --- DÜZENLEME (EDIT) KISMI ---

        // GET: /Books/Edit/5
        // Düzenleme sayfasını, o kitabın mevcut bilgileriyle açar
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: /Books/Edit/5
        // Değişiklikleri kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(book); // Güncelle
                await _context.SaveChangesAsync(); // Kaydet
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // --- SİLME (DELETE) KISMI ---

        // POST: /Books/Delete/5
        // Uyarı vermeden direkt siler (JavaScript ile onay alacağız)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book); // Sil
                await _context.SaveChangesAsync(); // Kaydet
            }
            return RedirectToAction(nameof(Index));
        }
    }
}