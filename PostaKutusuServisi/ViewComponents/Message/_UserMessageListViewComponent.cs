using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PostaKutusuServisi.Entities;
using PostaKutusuServisi.Models;

namespace PostaKutusuServisi.ViewComponents.Message
{
    public class _UserMessageListViewComponent(UserManager<AppUser> _userManager) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(List<UserMessage> messages, bool trashMode = false)
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

            var model = new UserMessageListViewModel
            {
                Messages = messages,
                CurrentUserId = user.Id,
                TrashMode = trashMode
            };

            return View(model);
        }
    }
}
