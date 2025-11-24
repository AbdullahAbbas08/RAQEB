namespace Raqeb.Shared.Models
{

namespace Raqeb.Shared.Models
    {
        public class PDMonthlyRowStat
        {
            [Key]
            public int Id { get; set; }
            public int PoolId { get; set; }
            public string PoolName { get; set; }
            public int Version { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }
            public int FromGrade { get; set; }
            public int TotalCount { get; set; }
            public double PDPercent { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }

    }


}
