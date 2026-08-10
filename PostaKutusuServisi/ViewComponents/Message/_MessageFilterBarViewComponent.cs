using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostaKutusuServisi.Context;
using PostaKutusuServisi.DTOs.UserMessageDto;
using PostaKutusuServisi.Entities;
using PostaKutusuServisi.Models;

namespace PostaKutusuServisi.ViewComponents.Message
{
    public class _MessageFilterBarViewComponent(UserManager<AppUser> _userManager,
                                                AppDbContext _context) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(MessageFilterDto filter, string action)
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            var model = new MessageFilterBarViewModel
            {
                Filter = filter,
                Action = action,
                Categories = await _context.Categories
                    .Where(c => c.UserId == user.Id)
                    .OrderBy(c => c.Name)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}
