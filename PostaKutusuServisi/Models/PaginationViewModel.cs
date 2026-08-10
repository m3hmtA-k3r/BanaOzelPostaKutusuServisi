using PostaKutusuServisi.DTOs.UserMessageDto;

namespace PostaKutusuServisi.Models
{
    public class PaginationViewModel
    {
        public MessageFilterDto Filter { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public string Action { get; set; }
    }
}
