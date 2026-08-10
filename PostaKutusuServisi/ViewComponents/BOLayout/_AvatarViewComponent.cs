using Microsoft.AspNetCore.Mvc;
using PostaKutusuServisi.Entities;
using PostaKutusuServisi.Models;

namespace PostaKutusuServisi.ViewComponents.BOLayout
{
    public class _AvatarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(AppUser user, string size = "md", string variant = "neutral")
        {
            var model = new AvatarViewModel
            {
                User = user,
                Size = size,
                Variant = variant
            };

            return View(model);
        }
    }
}
