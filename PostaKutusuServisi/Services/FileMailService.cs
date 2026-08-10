namespace PostaKutusuServisi.Services
{
    public class FileMailService(IWebHostEnvironment _environment) : IMailService
    {
        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var folder = Path.Combine(_environment.ContentRootPath, "sent-emails");
            Directory.CreateDirectory(folder);

            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.html";
            var filePath = Path.Combine(folder, fileName);

            var content = $@"<!doctype html>
                                <html lang=""tr"">
                                <head><meta charset=""utf-8""><title>{subject}</title></head>
                                <body style=""font-family:sans-serif;padding:24px"">
                                <p><b>Kime:</b> {to}</p>
                                <p><b>Konu:</b> {subject}</p>
                                <p><b>Tarih:</b> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
                                <hr>
                                    {htmlBody}
                                </body>
                                </html>";

            await File.WriteAllTextAsync(filePath, content);
        }
    }
}
