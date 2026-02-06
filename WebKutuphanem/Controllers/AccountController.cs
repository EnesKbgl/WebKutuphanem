using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebKutuphanem.ViewModels;

namespace WebKutuphanem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ============================================================
        // LOGIN (GİRİŞ) İŞLEMLERİ
        // ============================================================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Şifre kontrolü yap ve giriş yapmayı dene
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home"); // Başarılıysa Anasayfaya git
                }

                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            }
            return View(model);
        }

        // ============================================================
        // REGISTER (KAYIT) İŞLEMLERİ
        // ============================================================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // ============================================================
        // LOGOUT (ÇIKIŞ) İŞLEMİ - DÜZELTİLEN KISIM BURASI
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            // Çıkış yapınca direkt Login (Giriş) ekranına yönlendir:
            return RedirectToAction("Login", "Account");
        }
    }
}