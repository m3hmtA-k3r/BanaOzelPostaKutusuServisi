using Microsoft.AspNetCore.Mvc;

namespace PostaKutusuServisi.ViewComponents.BOLayout
{
    public class _LayoutHeaderViewComponent: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
