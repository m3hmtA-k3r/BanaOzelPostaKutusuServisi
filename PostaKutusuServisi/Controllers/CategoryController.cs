using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostaKutusuServisi.Context;
using PostaKutusuServisi.DTOs.CategoryDtos;
using PostaKutusuServisi.Entities;
using PostaKutusuServisi.Models;

namespace PostaKutusuServisi.Controllers
{
    [Authorize]
    public class CategoryController(UserManager<AppUser> _userManager,
                                    AppDbContext _context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var categories = await _context.Categories
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryListItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Color = c.Color,
                    MessageCount = c.Messages.Count(m => !m.IsDraft)
                })
                .ToListAsync();

            return View(categories);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CategoryDto categoryDto)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                TempData["CategoryError"] = "Kategori adı boş olamaz.";
                return RedirectToAction("Index");
            }

            var name = categoryDto.Name.Trim();

            var exists = await _context.Categories
                .AnyAsync(c => c.UserId == user.Id && c.Name == name);

            if (exists)
            {
                TempData["CategoryError"] = $"\"{name}\" adında bir kategoriniz zaten var.";
                return RedirectToAction("Index");
            }

            _context.Categories.Add(new Category
            {
                Name = name,
                Color = string.IsNullOrWhiteSpace(categoryDto.Color) ? "#3b72e0" : categoryDto.Color,
                UserId = user.Id
            });

            await _context.SaveChangesAsync();

            TempData["CategorySuccess"] = "Kategori oluşturuldu.";

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Update(CategoryDto categoryDto)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryDto.Id && c.UserId == user.Id);

            if (category is null)
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                TempData["CategoryError"] = "Kategori adı boş olamaz.";
                return RedirectToAction("Index");
            }

            category.Name = categoryDto.Name.Trim();
            category.Color = string.IsNullOrWhiteSpace(categoryDto.Color) ? category.Color : categoryDto.Color;

            await _context.SaveChangesAsync();

            TempData["CategorySuccess"] = "Kategori güncellendi.";

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (category is not null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["CategorySuccess"] = "Kategori silindi. Mesajlar silinmedi, kategorisiz kaldı.";
            }

            return RedirectToAction("Index");
        }
    }
}
