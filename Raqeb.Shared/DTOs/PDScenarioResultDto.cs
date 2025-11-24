namespace Raqeb.Shared.DTOs
{
    // 🔹 نتيجة حساب الـ PD لدرجة واحدة داخل سيناريو معيّن (Base / Best / Worst)
    public class PDScenarioResultDto
    {
        // 🔹 اسم السيناريو: Base / Best / Worst
        public string Scenario { get; set; }

        // 🔹 رقم الدرجة (1 / 2 / 3)
        public int Grade { get; set; }

        // 🔹 وصف الـ BUK (مثلاً: CURRENT 0 ، (1-30) ، (31-90))
        public string BUK { get; set; }

        // 🔹 TTC PD كـ Probability بين 0 و 1 (مثلاً 0.0063 = 0.63%)
        public double TTC_PD { get; set; }

        // 🔹 معامل الارتباط ρ (Asset Correlation)
        public double AssetCorrelation { get; set; }

        // 🔹 قيم PIT PD عند الآفاق t+1 .. t+5 (بالنسبة المئوية %)
        //     هذا الجدول يقابل الصورة اللي عنوانها "Cumulative PD" في الإكسل
        public List<double> PitPdByHorizon { get; set; } = new();

        // 🔹 قيم Survival عند t0 .. t5 (بالنسبة المئوية %)
        public List<double> SurvivalByHorizon { get; set; } = new();

        // 🔹 قيم Marginal PD في كل سنة (t+1 .. t+5) بالنسب المئوية %
        public List<double> MarginalPdByYear { get; set; } = new();
    }



    public class PDMarginalGroupedResponse
    {
        public List<PDScenarioResultDto>? Base { get; set; }
        public List<PDScenarioResultDto>? Best { get; set; }
        public List<PDScenarioResultDto>? Worst { get; set; }
    }

    public class MarginalPdTablesResponse
    {
        public List<MarginalPdRowDto> Base { get; set; }
        public List<MarginalPdRowDto> Best { get; set; }
        public List<MarginalPdRowDto> Worst { get; set; }
    }


    public class MarginalPdRowDto
    {
        public int Grade { get; set; }              // 1 / 2 / 3
        public string BUK { get; set; }             // CURRENT 0 / (1 - 30) / (31 - 90)

        // كل القيم دى هترجع كنسب مئوية منسّقة (مثال: "0.63%")
        public string TTC_PD { get; set; }          // TTC-PD
        public string AssetCorrelation { get; set; }// Asset Correlation

        public string PIT_T1 { get; set; }          // PIT-PD (t+1)
        public string PIT_T2 { get; set; }          // PIT-PD (t+2)
        public string PIT_T3 { get; set; }          // PIT-PD (t+3)
        public string PIT_T4 { get; set; }          // PIT-PD (t+4)
        public string PIT_T5 { get; set; }          // PIT-PD (t+5)
    }




}
