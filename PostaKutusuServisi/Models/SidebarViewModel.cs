using PostaKutusuServisi.Entities;

namespace PostaKutusuServisi.Models
{
    public class SidebarViewModel
    {
        public AppUser CurrentUser { get; set; }
        public int UnreadCount { get; set; }
        public List<Category> Categories { get; set; }
    }
}
