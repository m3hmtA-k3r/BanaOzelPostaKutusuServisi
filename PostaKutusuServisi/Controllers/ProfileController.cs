using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PostaKutusuServisi.DTOs.UserDtos;
using PostaKutusuServisi.Entities;

namespace PostaKutusuServisi.Controllers
{
    [Authorize]
    public class ProfileController(UserManager<AppUser> _userManager,
                                   SignInManager<AppUser> _signInManager,
                                   IWebHostEnvironment _environment) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var dto = new ProfileEditDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            ViewBag.User = user;

            return View(dto);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ProfileEditDto profileEditDto)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (string.IsNullOrWhiteSpace(profileEditDto.FirstName) ||
                string.IsNullOrWhiteSpace(profileEditDto.LastName))
            {
                TempData["ProfileError"] = "Ad ve soyad boş bırakılamaz.";
                return RedirectToAction("Index");
            }

            // --- fotoğraf ---
            if (profileEditDto.ProfileImage is not null && profileEditDto.ProfileImage.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(profileEditDto.ProfileImage.FileName).ToLowerInvariant();

                if (!allowed.Contains(extension))
                {
                    TempData["ProfileError"] = "Yalnızca jpg, png veya webp yükleyebilirsiniz.";
                    return RedirectToAction("Index");
                }

                if (profileEditDto.ProfileImage.Length > 2 * 1024 * 1024)
                {
                    TempData["ProfileError"] = "Dosya boyutu en fazla 2 MB olabilir.";
                    return RedirectToAction("Index");
                }

                var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "profile");
                Directory.CreateDirectory(uploadFolder);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileEditDto.ProfileImage.CopyToAsync(stream);
                }

                // eski dosyayı sil
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var oldPath = Path.Combine(_environment.WebRootPath,
                                               user.ProfileImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                user.ProfileImageUrl = $"/uploads/profile/{fileName}";
            }

            user.FirstName = profileEditDto.FirstName.Trim();
            user.LastName = profileEditDto.LastName.Trim();

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["ProfileError"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction("Index");
            }

            TempData["ProfileSuccess"] = "Profiliniz güncellendi.";

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
            {
                TempData["PasswordError"] = "Yeni şifreler birbiriyle uyuşmuyor.";
                return RedirectToAction("Index");
            }

            var result = await _userManager.ChangePasswordAsync(user,
                                                                changePasswordDto.CurrentPassword,
                                                                changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                TempData["PasswordError"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction("Index");
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["PasswordSuccess"] = "Şifreniz değiştirildi.";

            return RedirectToAction("Index");
        }
    }
}
