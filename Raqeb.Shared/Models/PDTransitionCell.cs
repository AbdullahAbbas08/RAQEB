using System.ComponentModel.DataAnnotations;

namespace Raqeb.Shared.Models
{
    public class PDTransitionCell
    {
        [Key]
        public int Id { get; set; }

        // 🔹 رقم الـ Pool
        public int PoolId { get; set; }

        // 🔹 اسم الـ Pool
        [MaxLength(200)]
        public string PoolName { get; set; }

        // 🔹 رقم النسخة (Version)
        public int Version { get; set; }

        // 🔹 موقع الخلية في المصفوفة
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }

        // 🔹 القيمة داخل الخلية
        public double Value { get; set; }

        // 🔹 وقت الإنشاء
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


}
