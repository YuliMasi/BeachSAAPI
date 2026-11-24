
using BeachSAAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BeachSAAPI.Dto
{
    /// <summary>
    /// Contexto de Entity Framework Core para la base de datos BeachSADB.
    /// </summary>
    public class DbContextBeachSA : DbContext
    {
        /// <summary>
        /// Crea el contexto con las opciones de conexión.
        /// </summary>
        /// <param name="options">Opciones de conexión a SQL Server.</param>
        public DbContextBeachSA(DbContextOptions<DbContextBeachSA> options) : base(options)
        {

        }

        /// <summary>
        /// Tabla de usuarios.
        /// </summary>
        public DbSet<User> Users { get; set; }
        /// <summary>
        /// Tabla de paquetes.
        /// </summary>
        public DbSet<Package> Packages { get; set; }
        /// <summary>
        /// Tabla de reservaciones.
        /// </summary>
        public DbSet<Reservation> Reservations { get; set; }

        /// <summary>
        /// Tabla de facturas.
        /// </summary>
        public DbSet<Invoice> Invoices { get; set; }


    }
}
