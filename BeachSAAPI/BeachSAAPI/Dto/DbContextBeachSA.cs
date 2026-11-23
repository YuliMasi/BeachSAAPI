
using BeachSAAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BeachSAAPI.Dto
{
    public class DbContextBeachSA : DbContext
    {
        public DbContextBeachSA(DbContextOptions<DbContextBeachSA> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Reservation> Reservations { get; set; } 
    }
}
