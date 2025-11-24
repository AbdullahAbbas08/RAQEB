// 🔹 جدول لتخزين قيم المتغير الماكرو لكل سيناريو (Base / Best / Worst)
public class MacroScenarioValue
{
    // 🔹 المفتاح الأساسي
    public int Id { get; set; }

    // 🔹 اسم السيناريو: Base / Best / Worst
    public string Scenario { get; set; }

    // 🔹 اسم المتغير (مثلاً Growth of real capital stock (%))
    public string VariableName { get; set; }

    // 🔹 السنة
    public int Year { get; set; }

    // 🔹 قيمة المتغير % في هذه السنة
    public double Value { get; set; }

    // 🔹 تاريخ الإنشاء
    public DateTime CreatedAt { get; set; }
}

// 🔹 جدول لتخزين Z-Index لكل سنة ولكل سيناريو
public class MacroScenarioIndex
{
    public int Id { get; set; }

    // 🔹 Base / Best / Worst
    public string Scenario { get; set; }

    // 🔹 السنة
    public int Year { get; set; }

    // 🔹 قيمة Z = (X - mean) / std
    public double ZValue { get; set; }

    public DateTime CreatedAt { get; set; }
}

// 🔹 تأكد إن جدول PDMarginalResults عندك يحتوي الأعمدة التالية أو ما يشابهها:
public class PDMarginalResult
{
    public int Id { get; set; }

    // 🔹 Base / Best / Worst
    public string Scenario { get; set; }

    // 🔹 درجة الائتمان 1 / 2 / 3
    public int Grade { get; set; }

    // 🔹 TTC PD كـ Probability (0–1)
    public double TTC_PD { get; set; }

    // 🔹 Asset correlation ρ
    public double AssetCorrelation { get; set; }

    // 🔹 Cumulative PD (t+1 .. t+5) كنِسب مئوية
    public double Cum1 { get; set; }
    public double Cum2 { get; set; }
    public double Cum3 { get; set; }
    public double Cum4 { get; set; }
    public double Cum5 { get; set; }

    // 🔹 Survival Probabilities (t0 .. t5) كنِسب مئوية
    public double Surv0 { get; set; }
    public double Surv1 { get; set; }
    public double Surv2 { get; set; }
    public double Surv3 { get; set; }
    public double Surv4 { get; set; }
    public double Surv5 { get; set; }

    // 🔹 Marginal PD لكل سنة (t+1 .. t+5) كنِسب مئوية
    public double PIT1 { get; set; }
    public double PIT2 { get; set; }
    public double PIT3 { get; set; }
    public double PIT4 { get; set; }
    public double PIT5 { get; set; }

    public DateTime CreatedAt { get; set; }
}
