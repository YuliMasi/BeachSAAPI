using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BeachSAAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<EmailService> logger;

        public EmailService(IConfiguration pConfig, ILogger<EmailService> pLogger)
        {
            this.configuration = pConfig;
            this.logger = pLogger;
        }

        public async Task<bool> SendInvoiceAsync(string toEmail, string toName, int invoiceNumber, byte[] pdf)
        {
            var settings = this.configuration.GetSection("EmailSettings");
            string host = settings["Host"];
            int port = settings.GetValue<int>("Port");
            string user = settings["User"];
            string password = settings["Password"];
            string fromName = settings["FromName"] ?? "Beach.SA";

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                this.logger.LogWarning("EmailSettings incompleto, no se envió la factura {InvoiceNumber}", invoiceNumber);
                return false;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, user));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = $"Beach.SA - Factura N° {invoiceNumber:D6}";

                var builder = new BodyBuilder
                {
                    HtmlBody = $"<p>Hola {toName},</p>" +
                               "<p>Gracias por reservar con Beach.SA. Adjuntamos la factura de su reservación.</p>" +
                               "<p>Beach.SA · Hoteles &amp; Resort</p>"
                };
                builder.Attachments.Add($"Factura-{invoiceNumber:D6}.pdf", pdf, new ContentType("application", "pdf"));
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(user, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "No se pudo enviar la factura {InvoiceNumber}", invoiceNumber);
                return false;
            }
        }
    }
}
