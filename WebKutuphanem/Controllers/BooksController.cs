using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // UserManager için gerekli
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json; // Google Books API'den gelen veriyi okumak için eklendi
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

        // ============================================================
        // GOOGLE BOOKS API İLE KİTAP ARAMA (SİHİRLİ MOTOR)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> SearchFromGoogle(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new { success = false, message = "Arama kelimesi boş olamaz." });

            // BURAYA KENDİ KOPYALADIĞIN API ANAHTARINI YAPIŞTIR (Tırnaklar kalsın)
            string apiKey = "AIzaSyC3-Cd-cygT9qtxF4oPF5HaC0wPTZbVTfM";

            // Linkin sonuna &key={apiKey} ekledik!
            string url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}&maxResults=1&key={apiKey}";

            using (var client = new HttpClient())
            {
                // ÇÖZÜM BURADA: Google bizi zararlı bir bot sanmasın diye sahte bir tarayıcı kimliği (User-Agent) ekliyoruz.
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                try
                {
                    var response = await client.GetAsync(url);

                    // Eğer Google isteği yinede reddederse, TAM hata kodunu (örn: 403, 429) ekrana yazdırıyoruz ki anlayalım.
                    if (!response.IsSuccessStatusCode)
                        return Json(new { success = false, message = $"API Bağlantı Hatası: {(int)response.StatusCode} - {response.ReasonPhrase}" });

                    var jsonString = await response.Content.ReadAsStringAsync();

                    using (var document = JsonDocument.Parse(jsonString))
                    {
                        var root = document.RootElement;

                        if (!root.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
                            return Json(new { success = false, message = "Kitap bulunamadı." });

                        var volumeInfo = items[0].GetProperty("volumeInfo");

                        string title = volumeInfo.TryGetProperty("title", out var t) ? t.GetString() : "";

                        string author = "";
                        if (volumeInfo.TryGetProperty("authors", out var authors) && authors.GetArrayLength() > 0)
                        {
                            author = authors[0].GetString();
                        }

                        int pageCount = volumeInfo.TryGetProperty("pageCount", out var pc) ? pc.GetInt32() : 0;
                        string description = volumeInfo.TryGetProperty("description", out var desc) ? desc.GetString() : "";
                        string publisher = volumeInfo.TryGetProperty("publisher", out var pub) ? pub.GetString() : "";

                        string coverImage = "";
                        if (volumeInfo.TryGetProperty("imageLinks", out var imageLinks))
                        {
                            coverImage = imageLinks.TryGetProperty("thumbnail", out var thumb) ? thumb.GetString() : "";
                            if (coverImage.StartsWith("http:")) coverImage = coverImage.Replace("http:", "https:");
                        }

                        string isbn = "";
                        if (volumeInfo.TryGetProperty("industryIdentifiers", out var identifiers))
                        {
                            foreach (var id in identifiers.EnumerateArray())
                            {
                                if (id.GetProperty("type").GetString() == "ISBN_13" || id.GetProperty("type").GetString() == "ISBN_10")
                                {
                                    isbn = id.GetProperty("identifier").GetString();
                                    break;
                                }
                            }
                        }

                        return Json(new
                        {
                            success = true,
                            title = title,
                            author = author,
                            pageCount = pageCount,
                            description = description,
                            publisher = publisher,
                            coverImage = coverImage,
                            isbn = isbn
                        });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Sistemsel Hata: " + ex.Message });
                }
            }
        }
    }
}