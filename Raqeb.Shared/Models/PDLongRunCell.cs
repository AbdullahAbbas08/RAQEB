namespace Raqeb.Shared.Models
{
    public class PDLongRunCell
    {
        [Key]
        public int Id { get; set; }
        public int PoolId { get; set; }

        [MaxLength(200)]
        public string PoolName { get; set; }

        public int Version { get; set; }

        // ✅ إعادة التسمية لتتطابق مع الاستخدام المنطقي
        public int FromGrade { get; set; }   // كان RowIndex
        public int ToGrade { get; set; }     // كان ColumnIndex

        public double Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
