namespace Raqeb.Shared.Models
{
    public class PDYearlyAverageCell
    {
        [Key]
        public int ID { get; set; }

        public int PoolId { get; set; }
        public string PoolName { get; set; }
        public int Version { get; set; }

        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public double Value { get; set; }

        public int Year { get; set; } // 👈 لتخزين السنة المحددة
        public DateTime CreatedAt { get; set; }
    }


}
