namespace PostaKutusuServisi.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Color { get; set; }

        public int UserId { get; set; }
        public AppUser User { get; set; }

        public List<UserMessage> Messages { get; set; }
    }
}
