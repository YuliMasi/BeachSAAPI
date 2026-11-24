using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeachSAAPI.Models
{
    /// <summary>
    /// Usuario del sistema; la cédula es su llave primaria.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Cédula del usuario; no es autoincremental.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        /// <summary>
        /// Tipo de cédula.
        /// </summary>
        public string IdType { get; set; }
        /// <summary>
        /// Nombre completo.
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// Número de teléfono.
        /// </summary>
        public int PhoneNumber { get; set; }
        /// <summary>
        /// Dirección.
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Correo electrónico, único, con el que inicia sesión.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Contraseña cifrada con SHA-256; puede venir vacía al actualizar.
        /// </summary>
        public string? Password { get; set; }
        /// <summary>
        /// Rol del usuario: Admin o Customer.
        /// </summary>
        public string Role { get; set; } //Admin, and Customer
    }
}
