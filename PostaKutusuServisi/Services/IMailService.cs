namespace PostaKutusuServisi.Services
{
    public interface IMailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }
}
