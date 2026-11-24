using BeachSAAPI.Models;

namespace BeachSAAPI.Services
{
    public interface IInvoicePdfService
    {
        byte[] Generate(Invoice invoice, Reservation reservation, User user, Package package);
    }
}
