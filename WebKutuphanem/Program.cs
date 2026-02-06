using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebKutuphanem.Data;
using WebKutuphanem.Localizations;

var builder = WebApplication.CreateBuilder(args);

// 1. VERİTABANI BAĞLANTISI
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. KİMLİK SİSTEMİ (IDENTITY) AYARLARI
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // --- ŞİFRE AYARLARI ---
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;

    // --- KULLANICI AYARLARI (Sorunu Çözen Kısım) ---
    options.User.RequireUniqueEmail = true;

    // Kullanıcı adında izin verilen karakterler (Türkçe harfler ve boşluk eklendi)
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ çÇğĞıİöÖşŞüÜ";
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<TurkishIdentityErrorDescriber>();

// 3. MVC SERVİSLERİ
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

// 4. KİMLİK VE YETKİ DENETİMİ
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();