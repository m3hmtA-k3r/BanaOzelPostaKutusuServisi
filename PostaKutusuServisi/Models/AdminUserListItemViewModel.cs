namespace PostaKutusuServisi.Models
{
    public class AdminUserListItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public int SentCount { get; set; }
    }
}
