using System.ComponentModel.DataAnnotations;

namespace BeachSAAPI.Models
{
    public class Package
    {

        [Key]
        public int ID { get; set; }

        public string PackageType { get; set; }
       
        public double Cost { get; set; }

        public double PercentageTotalAmount { get; set; }

        public int RemainingMonthlyInstallments { get; set; }
    
    }
}
