using System.ComponentModel.DataAnnotations;

namespace BeachSAAPI.Models
{
    public class Package
    {

        [Key]
        public int ID { get; set; }
        public double Cost { get; set; }
    }
}
