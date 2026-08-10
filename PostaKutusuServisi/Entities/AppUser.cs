using Microsoft.AspNetCore.Identity;

namespace PostaKutusuServisi.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public List<UserMessage> SentMessages { get; set; }
        public List<UserMessage> ReceiverMessages { get; set; }
        public List<Category> Categories { get; set; }
    }
}
