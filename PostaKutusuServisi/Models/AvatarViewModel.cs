using PostaKutusuServisi.Entities;

namespace PostaKutusuServisi.Models
{
    public class AvatarViewModel
    {
        public AppUser User { get; set; }
        public string Size { get; set; } = "md";
        public string Variant { get; set; } = "neutral";
    }
}
