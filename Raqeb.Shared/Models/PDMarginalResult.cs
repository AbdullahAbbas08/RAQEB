using System;
using System.ComponentModel.DataAnnotations;

namespace Raqeb.Shared.Models
{
    public class PDMarginalResult
    {
        [Key]
        public int Id { get; set; }

        public string Scenario { get; set; } = "";
        public int Grade { get; set; }
        public double TTC_PD { get; set; }
        public double AssetCorrelation { get; set; }
        public double PIT1 { get; set; }
        public double PIT2 { get; set; }
        public double PIT3 { get; set; }
        public double PIT4 { get; set; }
        public double PIT5 { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
