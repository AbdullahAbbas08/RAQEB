namespace Raqeb.Shared.Models
{
    public class PDLongRunAverage
    {
        public int Id { get; set; }
        public int FromGrade { get; set; }
        public int ToGrade { get; set; }
        public decimal Count { get; set; }
        public int YearCount { get; set; }
        public int AvgClients { get; set; }
        public decimal PDPercent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
