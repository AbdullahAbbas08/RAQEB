namespace Raqeb.Shared.DTOs
{
    // 🔹 إحصائيات الصفوف (إجمالي وعدد PD%)
    public class RowStatDto
    {
        public int FromGrade { get; set; }
        public int TotalCount { get; set; }
        public double PDPercent { get; set; }
    }
}
