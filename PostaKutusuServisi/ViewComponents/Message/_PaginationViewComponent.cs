using Microsoft.AspNetCore.Mvc;
using PostaKutusuServisi.DTOs.UserMessageDto;
using PostaKutusuServisi.Models;

namespace PostaKutusuServisi.ViewComponents.Message
{
    public class _PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(MessageFilterDto filter, int totalPages, int totalCount, string action)
        {
            var model = new PaginationViewModel
            {
                Filter = filter,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Action = action
            };

            return View(model);
        }
    }
}
