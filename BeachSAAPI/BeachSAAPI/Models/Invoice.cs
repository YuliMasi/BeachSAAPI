using System.ComponentModel.DataAnnotations;

namespace BeachSAAPI.Models
{
    /// <summary>
    /// Factura generada automáticamente para cada reservación.
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Identificador de la factura.
        /// </summary>
        [Key]
        public int ID { get; set; }

        // Llaves relacionadas
        /// <summary>
        /// Cédula del cliente facturado.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Identificador del paquete facturado.
        /// </summary>
        public int PackageId { get; set; }
        /// <summary>
        /// Identificador de la reservación; es único.
        /// </summary>
        public int ReservationId { get; set; }

        // Información de la factura
        /// <summary>
        /// Número consecutivo de la factura.
        /// </summary>
        public int InvoiceNumber { get; set; }
        /// <summary>
        /// Fecha de emisión.
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        // Detalle de los montos
        /// <summary>
        /// Monto antes de descuento e IVA, en dólares.
        /// </summary>
        public double Subtotal { get; set; }
        /// <summary>
        /// Descuento aplicado, en decimal.
        /// </summary>
        public double Discount { get; set; }
        /// <summary>
        /// Monto del descuento, en dólares.
        /// </summary>
        public double DiscountAmount { get; set; }
        /// <summary>
        /// IVA aplicado, en decimal.
        /// </summary>
        public double IVA { get; set; }
        /// <summary>
        /// Monto del IVA, en dólares.
        /// </summary>
        public double IvaAmount { get; set; }
        /// <summary>
        /// Tipo de cambio de venta del BCCR.
        /// </summary>
        public double ExchangeRate { get; set; }

        // Totales
        /// <summary>
        /// Total en dólares.
        /// </summary>
        public double totalUsd { get; set; }
        /// <summary>
        /// Total en colones.
        /// </summary>
        public double totalColones { get; set; }

    }
}
