namespace PostaKutusuServisi.Models
{
    public class AdminReportListItemViewModel
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public string ReporterName { get; set; }
        public string Reason { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
