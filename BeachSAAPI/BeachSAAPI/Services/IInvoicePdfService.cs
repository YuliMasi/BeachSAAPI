using BeachSAAPI.Models;

namespace BeachSAAPI.Services
{
    /// <summary>
    /// Define la generación del PDF de una factura.
    /// </summary>
    public interface IInvoicePdfService
    {
        /// <summary>
        /// Genera el PDF de una factura.
        /// </summary>
        /// <param name="invoice">Factura a generar o validar.</param>
        /// <param name="reservation">Reservación asociada.</param>
        /// <param name="user">Usuario para el que se genera el token.</param>
        /// <param name="package">Paquete asociado.</param>
        /// <returns>El contenido del archivo PDF.</returns>
        byte[] Generate(Invoice invoice, Reservation reservation, User user, Package package);
    }
}
