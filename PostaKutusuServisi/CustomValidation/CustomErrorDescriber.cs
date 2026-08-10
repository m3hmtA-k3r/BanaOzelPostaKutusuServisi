using Microsoft.AspNetCore.Identity;

namespace PostaKutusuServisi.CustomValidation
{
    public class CustomErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordRequiresDigit),
                Description = "🔢 Şifre en az 1 rakam içermeli."
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordRequiresLower),
                Description = "🔡 Şifre en az 1 küçük harf içermeli."
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordRequiresUpper),
                Description = "🔠 Şifre en az 1 büyük harf içermeli."
            };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "🔣 Şifre en az 1 özel karakter (*, !, vb.) içermeli."
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError()
            {
                Code = nameof(PasswordTooShort),
                Description = $"📏 Şifre en az {length} karakterden oluşmalı."
            };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError()
            {
                Code = nameof(DuplicateUserName),
                Description = $"🚫 '{userName}' kullanıcı adı daha önce alınmış."
            };
        }
    }
}