namespace Raqeb.Shared.Models.ECL
{
    public class EclCustomerInput
    {
        [Key]
        public int Id { get; set; }

        public int CustomerNumber { get; set; }
        public string CustomerName { get; set; }

        public decimal CreditLimit { get; set; }
        public decimal OutstandingBalance { get; set; }

        public decimal ScoreAtOrigination { get; set; }
        public decimal ScoreAtReporting { get; set; }

        public decimal? DPD { get; set; }

        public int PoolId { get; set; }

        public string Sector { get; set; }
        public string Group { get; set; }

        public DateTime FacilityStartDate { get; set; }

        // 🔵 NEW FIELDS — Risk Grades
        public int InitialRiskGrade { get; set; }
        public int CurrentRiskGrade { get; set; }

        // 🔵 NEW FIELDS — BUK
        public string Buk { get; set; }
        public int BukGrade { get; set; }

        // 🔵 NEW FIELDS — Staging (4 types)
        public string StageDpd { get; set; }
        public string StageSicr { get; set; }
        public string StageRating { get; set; }
        public string StageSpProvision { get; set; }

        // 🔵 NEW FIELDS — Final Stage
        public string FinalStage { get; set; }

        // 🔵 NEW FIELDS — EAD t+1 → t+5
        public decimal EAD_t1 { get; set; }
        public decimal EAD_t2 { get; set; }
        public decimal EAD_t3 { get; set; }
        public decimal EAD_t4 { get; set; }
        public decimal EAD_t5 { get; set; }

        public decimal ECL_Base_t1 { get; set; }
        public decimal ECL_Base_t2 { get; set; }
        public decimal ECL_Base_t3 { get; set; }
        public decimal ECL_Base_t4 { get; set; }
        public decimal ECL_Base_t5 { get; set; }

        public decimal ECL_Best_t1 { get; set; }
        public decimal ECL_Best_t2 { get; set; }
        public decimal ECL_Best_t3 { get; set; }
        public decimal ECL_Best_t4 { get; set; }
        public decimal ECL_Best_t5 { get; set; }

        public decimal ECL_Worst_t1 { get; set; }
        public decimal ECL_Worst_t2 { get; set; }
        public decimal ECL_Worst_t3 { get; set; }
        public decimal ECL_Worst_t4 { get; set; }
        public decimal ECL_Worst_t5 { get; set; }


        public decimal ECL_Base { get; set; }
        public decimal ECL_Best { get; set; }
        public decimal ECL_Worst { get; set; }
        public decimal ECL_Final { get; set; }
    }


    public class EclCcfInput 
    {
        [Key]
        public int Id { get; set; }
        public int PoolId { get; set; }
        public decimal UndrawnBalance { get; set; }
        public decimal CcfWeightedAvg { get; set; }
        public decimal ArithmeticMean { get; set; }
    }


    public class EclMacroeconomicInput
    {
        [Key]
        public int Id { get; set; }
        public int Year { get; set; }
        public decimal LendingRate { get; set; }
    }


    public class EclSicrMatrixInput
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Grade at Origination (1 → 6)
        /// </summary>
        public int OriginationGrade { get; set; }

        /// <summary>
        /// Grade at Reporting Date (1 → 6)
        /// </summary>
        public int ReportingGrade { get; set; }

        /// <summary>
        /// Output Stage (Stage 1 / Stage 2 / Stage 3)
        /// </summary>
        public string Stage { get; set; }
    }


    public class EclCureRateInput
    {
        [Key]
        public int Id { get; set; }
        public int PoolId { get; set; }
        public decimal CureRate { get; set; }
    }

    public class EclScoreGrade
    {
        [Key]
        public int Id { get; set; }
        public string ScoreGrade { get; set; }
        public string ScoreInterval { get; set; }
        public string RiskLevel { get; set; }
        public int RiskGrade { get; set; }
        public string Stage { get; set; }
    }

    public class EclDpdBucket
    {
        [Key]
        public int Id { get; set; }
        public int Dpd { get; set; }
        public string Bucket { get; set; }
        public int BucketGrade { get; set; }
    }


    public class EclScenarioWeight
    {
        [Key]
        public int Id { get; set; }
        public string Scenario { get; set; }
        public decimal WeightPercent { get; set; }
    }


    public class EclStageSummary
    {
        [Key]
        public int Id { get; set; }
        public string Stage { get; set; }
        public decimal Outstanding { get; set; }
        public decimal ECL { get; set; }
        public decimal LossRatio => Outstanding == 0 ? 0 : ECL / Outstanding;
        public decimal OSContribution { get; set; }
    }


    public class EclGradeSummary
    {
        [Key]
        public int Id { get; set; }
        public decimal Grade { get; set; }
        public decimal Outstanding { get; set; }
        public decimal ECL { get; set; }
        public decimal LossRatio => Outstanding == 0 ? 0 : ECL / Outstanding;
    }


    public class CustomerWithStage
    {
        public int CustomerId { get; set; }
        public int CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal Outstanding { get; set; }
        public decimal ECL_Final { get; set; }

    }


    public class PaginationRequest
    {
        public int Page { get; set; } = 1;      // default page
        public int PageSize { get; set; } = 20; // default size
    }

    public class PaginatedResponse<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRows { get; set; }
        public int TotalPages { get; set; }
        public List<T> Data { get; set; }
    }

    public class CustomerStageFilterRequest
    {
        public int? CustomerNumber { get; set; }
        public string? CustomerName { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

}
