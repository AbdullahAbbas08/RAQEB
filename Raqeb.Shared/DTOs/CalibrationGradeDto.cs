namespace Raqeb.Shared.DTOs
{
    public class CalibrationGradeDto
    {
        public int Grade { get; set; }
        public double ODR { get; set; }           // %
        public double LnOdds { get; set; }
        public double FittedLnOdds { get; set; }
        public double FittedPD { get; set; }      // %
        public double CFittedLnOdds { get; set; }
        public double CFittedPD { get; set; }     // %
        public int Count { get; set; }
    }

}
