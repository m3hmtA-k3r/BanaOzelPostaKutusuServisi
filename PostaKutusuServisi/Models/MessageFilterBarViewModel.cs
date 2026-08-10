using PostaKutusuServisi.DTOs.UserMessageDto;
using PostaKutusuServisi.Entities;

namespace PostaKutusuServisi.Models
{
    public class MessageFilterBarViewModel
    {
        public MessageFilterDto Filter { get; set; }
        public string Action { get; set; }
        public List<Category> Categories { get; set; }
    }
}
