namespace Raqeb.Shared.Models
{
    public class PDCalibrationResult
    {
        [Key]
        public int Id { get; set; }
        public int PoolId { get; set; }
        public int Year { get; set; }
        public int Grade { get; set; }
        public int Count { get; set; }
        public decimal ODRPercent { get; set; }
        public double LnOdds { get; set; }
        public double FittedLnOdds { get; set; }
        public decimal FittedPDPercent { get; set; }
        public double CFittedLnOdds { get; set; }
        public decimal CFittedPDPercent { get; set; }
        public double Intercept { get; set; }
        public double Slope { get; set; }
        public double CIntercept { get; set; }
        public DateTime CreatedAt { get; set; }

        public decimal? PortfolioPD { get; set; }     // إجمالي Portfolio PD (1%)
        public int? TotalCount { get; set; }           // إجمالي العملاء (14531)


    }



}
