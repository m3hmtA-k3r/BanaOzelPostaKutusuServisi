using System.Net;
using System.Net.Mail;

namespace PostaKutusuServisi.Services
{
    public class SmtpMailService(IConfiguration _configuration) : IMailService
    {
        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var host = _configuration["Smtp:Host"];
            var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var user = _configuration["Smtp:User"];
            var password = _configuration["Smtp:Password"];
            var from = _configuration["Smtp:From"] ?? user;
            var displayName = _configuration["Smtp:DisplayName"] ?? "PostaKutusuServisi";

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(from!, displayName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(to);

            await client.SendMailAsync(message);
        }
    }
}
