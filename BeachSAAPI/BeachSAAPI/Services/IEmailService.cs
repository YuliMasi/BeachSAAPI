namespace BeachSAAPI.Services
{
    /// <summary>
    /// Define el envío de facturas por correo.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía la factura en PDF al correo del cliente.
        /// </summary>
        /// <param name="toEmail">Correo del destinatario.</param>
        /// <param name="toName">Nombre del destinatario.</param>
        /// <param name="invoiceNumber">Número de la factura.</param>
        /// <param name="pdf">Contenido del PDF adjunto.</param>
        /// <returns>true si el correo se envió.</returns>
        Task<bool> SendInvoiceAsync(string toEmail, string toName, int invoiceNumber, byte[] pdf);
    }
}
