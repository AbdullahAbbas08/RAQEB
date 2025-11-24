namespace Raqeb.Shared.DTOs
{
    public class PDMatrixFilterDto
    {
        public int PoolId { get; set; }          // رقم الـ Pool
        public int Version { get; set; }         // رقم الإصدار

        public int? Year { get; set; }           // فلتر اختياري بالسنة
        public int? Month { get; set; }          // فلتر اختياري بالشهر

        public int Page { get; set; } = 1;       // رقم الصفحة (افتراضي = 1)
        public int PageSize { get; set; } = 6;   // عدد العناصر في الصفحة

        public int MinGrade { get; set; } = 1;   // أقل درجة
        public int MaxGrade { get; set; } = 4;   // أعلى درجة
    }
}
