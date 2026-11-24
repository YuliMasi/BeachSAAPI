using System.ComponentModel.DataAnnotations;

namespace BeachSAAPI.Models
{
    /// <summary>
    /// Paquete vacacional que ofrece el hotel.
    /// </summary>
    public class Package
    {

        /// <summary>
        /// Identificador del paquete.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Tipo de paquete: Todo incluido, Alimentación u Hospedaje.
        /// </summary>
        public string PackageType { get; set; }
       
        /// <summary>
        /// Precio por persona por noche, en dólares.
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        /// Porcentaje del total que se paga como prima.
        /// </summary>
        public double PercentageTotalAmount { get; set; }

        /// <summary>
        /// Cantidad de mensualidades para el saldo.
        /// </summary>
        public int RemainingMonthlyInstallments { get; set; }
    
    }
}
