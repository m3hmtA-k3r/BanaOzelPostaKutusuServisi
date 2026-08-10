namespace PostaKutusuServisi.Entities
{
    public class MessageReport
    {
        public int Id { get; set; }

        public int MessageId { get; set; }
        public UserMessage Message { get; set; }

        public int ReportedByUserId { get; set; }
        public AppUser ReportedByUser { get; set; }

        public string Reason { get; set; }
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
