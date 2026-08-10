using PostaKutusuServisi.Entities;

namespace PostaKutusuServisi.Models
{
    public class UserMessageListViewModel
    {
        public List<UserMessage> Messages { get; set; }
        public int CurrentUserId { get; set; }
        public bool TrashMode { get; set; }
    }
}
