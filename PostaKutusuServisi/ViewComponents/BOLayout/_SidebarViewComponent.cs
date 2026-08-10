using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostaKutusuServisi.Context;
using PostaKutusuServisi.Entities;
using PostaKutusuServisi.Models;

namespace PostaKutusuServisi.ViewComponents.BOLayout
{
    public class _SidebarViewComponent(UserManager<AppUser> _userManager,
                                       AppDbContext _context) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            var unreadCount = await _context.UserMessages
                .CountAsync(x => x.ReceiverId == user.Id
                              && !x.IsRead
                              && !x.IsDraft
                              && !x.IsDeletedByReceiver);

            var model = new SidebarViewModel
            {
                CurrentUser = user,
                UnreadCount = unreadCount,
                Categories = await _context.Categories
                .Where(c => c.UserId == user.Id)
                .OrderBy(c => c.Name)
                .ToListAsync()
            };
    

            return View(model);
        }
    }
}
