namespace Raqeb.Shared.Models
{
    public class PDMonthlyTransitionCell
    {
        [Key]
        public int ID { get; set; }

        public int PoolId { get; set; }
        public string PoolName { get; set; }
        public int Version { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public double Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }


}
