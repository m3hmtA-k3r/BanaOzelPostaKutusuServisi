using Microsoft.AspNetCore.Mvc;
using PostaKutusuServisi.Models;

namespace PostaKutusuServisi.ViewComponents.BOLayout
{
    public class _UserMessageUstBaslikViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string fullName, int unreadCount)
        {
            var model = new UstBaslikViewModel
            {
                FullName = fullName,
                UnreadCount = unreadCount
            };

            return View(model);
        }
    }
}
