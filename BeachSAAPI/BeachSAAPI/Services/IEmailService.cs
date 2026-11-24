namespace BeachSAAPI.Services
{
    public interface IEmailService
    {
        Task<bool> SendInvoiceAsync(string toEmail, string toName, int invoiceNumber, byte[] pdf);
    }
}
