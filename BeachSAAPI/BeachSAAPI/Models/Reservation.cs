using System.ComponentModel.DataAnnotations;

namespace BeachSAAPI.Models
{
    /// <summary>
    /// Reservación de un paquete hecha por un cliente.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Identificador de la reservación.
        /// </summary>
        [Key]
        public int ID { get; set; }
        // Foreign keys
        /// <summary>
        /// Cédula del cliente.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Identificador del paquete.
        /// </summary>
        public int PackageId { get; set; }
        //normal properties
        /// <summary>
        /// Cantidad de noches.
        /// </summary>
        public int AccommodationDays { get; set; }
        /// <summary>
        /// Descuento aplicado, en decimal.
        /// </summary>
        public double Discount { get; set; }
        /// <summary>
        /// IVA aplicado, en decimal.
        /// </summary>
        public double IVA { get; set; }
        /// <summary>
        /// Forma de pago: efectivo, tarjeta o cheque.
        /// </summary>
        public string PaymentMethod { get; set; }
        /// <summary>
        /// Número del cheque, solo con pago en cheque.
        /// </summary>
        public string? CheckNumber { get; set; }
        /// <summary>
        /// Banco del cheque, solo con pago en cheque.
        /// </summary>
        public string? Bank { get; set; }
        /// <summary>
        /// Cantidad de personas.
        /// </summary>
        public int GuestQuantity { get; set; }
        /// <summary>
        /// Fecha de entrada.
        /// </summary>
        public DateTime CheckInDate { get; set; }
        /// <summary>
        /// Fecha de salida, calculada con las noches.
        /// </summary>
        public DateTime CheckOutDate { get; set; }

    }

}