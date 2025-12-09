namespace Raqeb.Shared.Models.ECL_SEMP
{
    public class ECLSEMPReceivableAging
    {
        [Key]
        public long Id { get; set; }
        public DateTime MonthYear { get; set; }
        public string Bucket { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ECLSEMPWriteOffNotRecognized
    {
        [Key]
        public long Id { get; set; }
        public DateTime MonthYear { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    public class ECLSEMPReceivableAgingSummary
    {
        [Key]
        public long Id { get; set; }
        public DateTime MonthYear { get; set; }
        public string Bucket { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }



    public class ECLSEMPFlowRateMatrix
    {
        [Key]
        public long Id { get; set; }

        public DateTime MonthYear { get; set; }  // العمود (الشهر) مثل 2021-04-01

        public string Bucket { get; set; } = null!;
        // rows: Not due, 0-30, 31-60, 61-90, 90+

        public decimal? Rate { get; set; }       // نخزن كنسبة 0..1 (مثال 0.25 = 25%)
        public decimal? RatePercent { get; set; } // اختياري: 0..100 (لو تحب تعرضها مباشرة)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    public class ECLSEMPWeightedAvgFlowRateMatrix
    {
        [Key]
        public long Id { get; set; }

        public int Duration { get; set; }     // 1..44
        public string Bucket { get; set; } = null!; // 0-30, 31-60, 61-90, 90+ (وNot due لو عايزها null)

        public decimal? Rate { get; set; }        // 0..1
        public decimal? RatePercent { get; set; } // 0..100

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ECLSEMPAvgLossRate
    {
        public long Id { get; set; }

        public int Duration { get; set; } // 1..44
        public string Bucket { get; set; } = null!; // 0-30,31-60,61-90,90+ (وNot due لو تحب)

        public decimal? Rate { get; set; }        // 0..1
        public decimal? RatePercent { get; set; } // 0..100

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }



    public class ECLSEMPTTCLossRate
    {
        public long Id { get; set; }

        public string Bucket { get; set; } = null!;   // Not due, 0-30, 31-60, 61-90, 90+

        public decimal? LossRate { get; set; }        // 0..1 (Average of durations)
        public decimal? LossRatePercent { get; set; } // 0..100

        public decimal? AnnualizedLossRate { get; set; }        // 0..1
        public decimal? AnnualizedLossRatePercent { get; set; } // 0..100

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ECLSEMPAnnualMeDatum
    {
        public long Id { get; set; }

        public int RowNo { get; set; }
        public string SubjectDescriptor { get; set; } = null!;

        public int Year { get; set; }
        public decimal? Value { get; set; }

        public bool? IsNegativeCorrelation { get; set; }

        public decimal? Weight { get; set; } // ✅ مثال: 0.8 يعني 80%

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }



    public class ECLSEMPAnnualMeScenario
    {
        public long Id { get; set; }

        public int RowNo { get; set; }                 // 85 أو 23
        public string SubjectDescriptor { get; set; } = null!;

        public int BaseYear { get; set; }              // 2025
        public decimal? BaseValue { get; set; }        // قيمة 2025

        public decimal? StdDev { get; set; }           // الانحراف المعياري (على historical)
        public bool? IsNegativeCorrelation { get; set; }

        public decimal? BestValue { get; set; }
        public decimal? WorstValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    public class ECLSEMPStandardizedAnnualMeScenario
    {
        public long Id { get; set; }

        public int RowNo { get; set; }                 // 85 / 23 / ... الخ
        public string SubjectDescriptor { get; set; } = null!;

        public int Year { get; set; }                  // 2018..2024 أو 2025 للـ Base
        public string ScenarioType { get; set; } = null!; // "HIST" / "BASE" / "BEST" / "WORST"

        public decimal? StandardizedValue { get; set; } // القيمة بعد Standardize (unit free)

        public decimal? Mean { get; set; }             // Average(M:V)
        public decimal? StdDev { get; set; }           // STDEV.S(M:V)

        public bool IsPlusCorrelation { get; set; }    // K12 == "+"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }



    public class ECLSEMPAnnualMeWeightedAvg
    {
        public long Id { get; set; }

        public int Year { get; set; }                    // 2018..2024 أو 2025
        public string ScenarioType { get; set; } = null!; // "HIST" / "BASE" / "BEST" / "WORST"

        public decimal? WeightedAverage { get; set; }     // الناتج

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }




    public class ECLSEMPAssetCorrelation
    {
        public long Id { get; set; }
        public string Bucket { get; set; } = null!; // Not due, 0-30, ...
        public decimal? AssetCorrelation { get; set; } // R
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    public class ECLSEMPPITLossRate
    {
        public long Id { get; set; }
        public string Bucket { get; set; } = null!;

        public decimal? Base { get; set; }
        public decimal? Best { get; set; }
        public decimal? Worst { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }



    public class ECLSEMPWeightsPreRecovery
    {
        public long Id { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }              // 1..12
        public DateTime AsOfDate { get; set; }      // آخر يوم في الشهر (اختياري لكن مفيد)

        public string Bucket { get; set; } = null!; // "91-120".."181-210"
        public decimal? Weight { get; set; }        // 0..1

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }



    public class ECLSEMPRecoveriesPost360Plus
    {
        public long Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime AsOfDate { get; set; } // آخر يوم في الشهر
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ECLSEMPRecoverabilityRatio
    {
        public long Id { get; set; }

        public DateTime MonthYear { get; set; }   // نخزن أول يوم في الشهر
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime AsOfDate { get; set; }    // آخر يوم في الشهر

        public string Bucket { get; set; } = null!; // "91-120","121-150","151-180","360+"
        public decimal? Ratio { get; set; }         // (current / lag6) - 1 OR special for 360+

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    public class ECLSEMPRecoverabilityExpectedValue
    {
        public long Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime AsOfDate { get; set; } // end of month
        public decimal? ExpectedValue { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    public class ECLSEMPRecoverabilityExpectedValueYearAvg
    {
        public long Id { get; set; }

        public int? Year { get; set; } // خليها nullable عشان historical
        public bool IsHistorical { get; set; } // true للسطر ده

        public decimal AvgExpectedValue { get; set; }
        public int MonthsCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    public class ECLSEMPCorporateEclSummary
    {
        public long Id { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }                 // 1..12
        public DateTime AsOfDate { get; set; }         // آخر يوم في الشهر

        public string Bucket { get; set; } = null!;    // Not due, 0-30, ... , 90+, WriteOff, Total

        public decimal ReceivableBalance { get; set; } // Col B
        public decimal EclBase { get; set; }           // Col C
        public decimal EclBest { get; set; }           // Col D
        public decimal EclWorst { get; set; }          // Col E
        public decimal EclWeightedAverage { get; set; }// Col F
        public decimal LossRatio { get; set; }         // Col G  (0..1)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    public class CorporateEclRowDto
    {
        public string Bucket { get; set; } = null!;
        public decimal ReceivableBalance { get; set; }
        public decimal EclBase { get; set; }
        public decimal EclBest { get; set; }
        public decimal EclWorst { get; set; }
        public decimal EclWeightedAverage { get; set; }
        public decimal LossRatio { get; set; } // stored as percentage (e.g., 1.58)
    }

    public class CorporateEclTableDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime AsOfDate { get; set; }
        public List<CorporateEclRowDto> Rows { get; set; } = new();
    }






}
