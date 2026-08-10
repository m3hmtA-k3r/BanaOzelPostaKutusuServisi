namespace PostaKutusuServisi.DTOs.UserMessageDto
{
    public class MessageFilterDto
    {
        public string? Search { get; set; }
        public string? ReadStatus { get; set; }       
        public bool OnlyImportant { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Sort { get; set; }            
        public int Page { get; set; } = 1;
        public int? CategoryId { get; set; }

    }
}
