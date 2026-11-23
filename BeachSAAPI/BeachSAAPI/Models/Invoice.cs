using System.ComponentModel.DataAnnotations;

namespace BeachSAAPI.Models
{
    public class Invoice
    {
        [Key]
        public int ID { get; set; }
        // Foreign keys
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public int ReservationId { get; set; }
        //normal properties
        public int InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public double totalUsd { get; set; }
        public double totalColones { get; set; }

    }
}
