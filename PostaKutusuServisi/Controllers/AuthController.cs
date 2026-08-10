using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PostaKutusuServisi.DTOs.UserDtos;
using PostaKutusuServisi.Entities;
using PostaKutusuServisi.Services;
using System.Threading.Tasks;

namespace PostaKutusuServisi.Controllers
{
    public class AuthController(UserManager<AppUser> _userManager,
                               SignInManager<AppUser> _signInManager,
                               IMailService _mailService) : Controller
    {
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserDto registerUser)
        {
            if(registerUser.Password != registerUser.ConfirmedPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View(registerUser);
            }

            var user = new AppUser
            {
                Email = registerUser.Email,
                FirstName = registerUser.FirstName,
                LastName = registerUser.LastName,
                UserName = registerUser.UserName
            };

            var result = await _userManager.CreateAsync(user, registerUser.Password);

            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return View(registerUser);
            }
            await _userManager.AddToRoleAsync(user, "User");

            var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmLink = Url.Action("ConfirmEmail", "Auth",
                                         new { userId = user.Id, token = confirmToken },
                                         Request.Scheme);

            var confirmBody = $@"
                <p>Merhaba {user.FirstName},</p>
                <p>PostaKutusuServisi'ne hoş geldiniz. Hesabınızı kullanmaya başlamadan önce
                   e-posta adresinizi doğrulamanız gerekiyor:</p>
                <p><a href=""{confirmLink}"">E-posta adresimi doğrula</a></p>
                <p>Bu hesabı siz oluşturmadıysanız bu e-postayı yok sayabilirsiniz.</p>
                <p style=""color:#888;font-size:12px"">PostaKutusuServisi</p>";

            await _mailService.SendAsync(user.Email!, "E-posta Adresinizi Doğrulayın", confirmBody);

            TempData["LoginInfo"] = "Kaydınız oluşturuldu. E-posta adresinize doğrulama bağlantısı gönderildi.";

            return RedirectToAction("Login");

        }

        public IActionResult Login()
        {
             return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Login);

            if (user == null)
            {
                user = await _userManager.FindByNameAsync(loginDto.Login);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "E-posta/kullanıcı adı veya şifre hatalı.");
                return View(loginDto);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,
                    "Çok fazla hatalı deneme yapıldı. Hesabınız 15 dakika süreyle kilitlendi.");
                return View(loginDto);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "E-posta/kullanıcı adı veya şifre hatalı.");
                return View(loginDto);
            }

            if (!user.IsActive)
            {
                await _signInManager.SignOutAsync();

                ModelState.AddModelError(string.Empty,
                    "Hesabınız devre dışı bırakılmış. Yönetici ile iletişime geçin.");
                return View(loginDto);
            }

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");

        }

        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            if (string.IsNullOrWhiteSpace(forgotPasswordDto.Email))
            {
                ModelState.AddModelError(string.Empty, "E-posta adresi gerekli.");
                return View(forgotPasswordDto);
            }

            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

            if (user is not null && user.IsActive)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var resetLink = Url.Action("ResetPassword", "Auth",
                                           new { email = user.Email, token },
                                           Request.Scheme);

                var body = $@"
                    <p>Merhaba {user.FirstName},</p>
                    <p>Hesabınız için şifre sıfırlama talebinde bulunuldu.
                       Aşağıdaki bağlantıya tıklayarak yeni şifrenizi belirleyebilirsiniz:</p>
                    <p><a href=""{resetLink}"">Şifremi sıfırla</a></p>
                    <p>Bu talebi siz yapmadıysanız bu e-postayı yok sayabilirsiniz;
                       şifreniz değişmeyecektir.</p>
                    <p style=""color:#888;font-size:12px"">PostaKutusuServisi</p>";

                await _mailService.SendAsync(user.Email!, "Şifre Sıfırlama", body);
            }

            TempData["ForgotInfo"] = "Bu e-posta sistemde kayıtlıysa, sıfırlama bağlantısı gönderildi.";

            return RedirectToAction("ForgotPassword");
        }


        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login");
            }

            var dto = new ResetPasswordDto
            {
                Email = email,
                Token = token
            };

            return View(dto);
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Şifreler birbiriyle uyuşmuyor.");
                return View(resetPasswordDto);
            }

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Bağlantı geçersiz.");
                return View(resetPasswordDto);
            }

            var result = await _userManager.ResetPasswordAsync(user,
                                                               resetPasswordDto.Token,
                                                               resetPasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(resetPasswordDto);
            }

            TempData["LoginInfo"] = "Şifreniz güncellendi. Yeni şifrenizle giriş yapabilirsiniz.";

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> ConfirmEmail(int userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null || string.IsNullOrWhiteSpace(token))
            {
                TempData["LoginInfo"] = "Doğrulama bağlantısı geçersiz.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            TempData["LoginInfo"] = result.Succeeded
                ? "E-posta adresiniz doğrulandı. Artık giriş yapabilirsiniz."
                : "Doğrulama bağlantısı geçersiz veya süresi dolmuş.";

            return RedirectToAction("Login");
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ResendConfirmation()
        {
            var user = await _userManager.FindByNameAsync(User.Identity!.Name!);

            if (user is null || user.EmailConfirmed)
            {
                return RedirectToAction("Index", "Message");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action("ConfirmEmail", "Auth",
                                  new { userId = user.Id, token },
                                  Request.Scheme);

            var body = $@"
                <p>Merhaba {user.FirstName},</p>
                <p>E-posta adresinizi doğrulamak için aşağıdaki bağlantıya tıklayın:</p>
                <p><a href=""{link}"">E-posta adresimi doğrula</a></p>
                <p style=""color:#888;font-size:12px"">PostaKutusuServisi</p>";

            await _mailService.SendAsync(user.Email!, "E-posta Adresinizi Doğrulayın", body);

            TempData["ConfirmationSent"] = "Doğrulama bağlantısı e-posta adresinize tekrar gönderildi.";

            return RedirectToAction("Index", "Message");
        }



    }

}
