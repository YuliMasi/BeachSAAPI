using System.ComponentModel.DataAnnotations;

namespace BeachSAAPI.Models
{
    public class Reservation
    {
        [Key]
        public int ID { get; set; }
        // Foreign keys
        public int UserId { get; set; }
        public int PackageId { get; set; }
        //normal properties
        public int AccommodationDays { get; set; }
        public double Discount { get; set; }
        public double IVA { get; set; }
        public string PaymentMethod { get; set; }
        public string? CheckNumber { get; set; }
        public string? Bank { get; set; }
        public int GuestQuantity { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

    }

}