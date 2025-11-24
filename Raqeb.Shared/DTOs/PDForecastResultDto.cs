namespace Raqeb.Shared.DTOs
{
    /// <summary>
    /// 🔹 نموذج بيانات لنتائج التنبؤ بمعدلات PD المستقبلية
    /// يشمل السيناريوهات الثلاثة (Base, Best, Worst)
    /// </summary>
    public class PDForecastResultDto
    {
        public int Year { get; set; }          // السنة المتنبأ بها
        public double BasePD { get; set; }     // السيناريو الأساسي
        public double BestPD { get; set; }     // السيناريو الأفضل (تحسن اقتصادي)
        public double WorstPD { get; set; }    // السيناريو الأسوأ (تدهور اقتصادي)
        public double MacroEffect { get; set; } // التأثير الاقتصادي المستخدم في الحساب
    }

    //public class PDForecastResultDto
    //{
    //    public string Scenario { get; set; }
    //    public double MacroEffect { get; set; }
    //    public string Description { get; set; }
    //}


}
