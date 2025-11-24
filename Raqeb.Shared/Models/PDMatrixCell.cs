namespace Raqeb.Shared.Models
{
    public class PDMatrixCell
    {
        [Key]
        public int Id { get; set; }

        // 🔹 رقم النسخة (Version)
        public int Version { get; set; }

        // 🔹 رقم الـ Pool المرتبط
        public int PoolId { get; set; }

        // 🔹 اسم الـ Pool
        [MaxLength(150)]
        public string PoolName { get; set; }

        // 🔹 نوع المصفوفة (Transition / Average / LongRun)
        [MaxLength(50)]
        public string MatrixType { get; set; }

        // 🔹 رقم الصف في المصفوفة
        public int RowIndex { get; set; }

        // 🔹 رقم العمود في المصفوفة
        public int ColumnIndex { get; set; }

        // 🔹 القيمة داخل المصفوفة
        public double Value { get; set; }

        // 🔹 تاريخ الإنشاء
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
