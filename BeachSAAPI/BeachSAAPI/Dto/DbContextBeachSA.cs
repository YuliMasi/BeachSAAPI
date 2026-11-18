
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
    }
}
