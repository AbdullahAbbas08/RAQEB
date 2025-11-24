namespace Raqeb.Shared.DTOs
{
    public class CalibrationSummaryDto
    {
        public double Intercept { get; set; }
        public double Slope { get; set; }
        public double CIntercept { get; set; }
        public List<CalibrationGradeDto> Grades { get; set; } = new();
        public double PortfolioPD { get; set; }   // المتوسط النهائي (زي 1%)
        public int TotalCount { get; set; }       // المجموع الكلي للـ Counts
    }

}
