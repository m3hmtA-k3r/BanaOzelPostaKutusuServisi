namespace PostaKutusuServisi.DTOs.UserMessageDto
{
    public class SendMailDto
    {
        public int? DraftId { get; set; }
        public string ReceiverMail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int? CategoryId { get; set; }
    }
}
