using Microsoft.AspNetCore.Identity;

namespace WebKutuphanem.Localizations
{
    // Bu sınıf, Identity'nin İngilizce hatalarını ezip yerine Türkçe döndürür.
    public class TurkishIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError()
        {
            return new IdentityError { Code = nameof(DefaultError), Description = "Bilinmeyen bir hata oluştu." };
        }

        public override IdentityError ConcurrencyFailure()
        {
            return new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Veri başka bir kullanıcı tarafından değiştirildi." };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError { Code = nameof(PasswordTooShort), Description = $"Şifre en az {length} karakter olmalıdır." };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Şifre en az bir sembol (*, !, - vb.) içermelidir." };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Şifre en az bir rakam (0-9) içermelidir." };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Şifre en az bir küçük harf (a-z) içermelidir." };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Şifre en az bir büyük harf (A-Z) içermelidir." };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError { Code = nameof(DuplicateUserName), Description = $"'{userName}' kullanıcı adı zaten kullanılıyor." };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError { Code = nameof(DuplicateEmail), Description = $"'{email}' e-posta adresi zaten kayıtlı." };
        }

        public override IdentityError InvalidUserName(string userName)
        {
            return new IdentityError { Code = nameof(InvalidUserName), Description = $"Geçersiz kullanıcı adı: '{userName}' (Sadece harf ve rakam kullanılabilir)." };
        }

        public override IdentityError InvalidEmail(string email)
        {
            return new IdentityError { Code = nameof(InvalidEmail), Description = $"Geçersiz e-posta adresi: '{email}'" };
        }
    }
}