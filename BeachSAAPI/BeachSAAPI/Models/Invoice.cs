using System.ComponentModel.DataAnnotations;

namespace BeachSAAPI.Models
{
    public class Invoice
    {
        [Key]
        public int ID { get; set; }

        // Llaves relacionadas
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public int ReservationId { get; set; }

        // Información de la factura
        public int InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }

        // Detalle de los montos
        public double Subtotal { get; set; }
        public double Discount { get; set; }
        public double DiscountAmount { get; set; }
        public double IVA { get; set; }
        public double IvaAmount { get; set; }
        public double ExchangeRate { get; set; }

        // Totales
        public double totalUsd { get; set; }
        public double totalColones { get; set; }

    }
}
