using System.ComponentModel.DataAnnotations;

namespace Raqeb.Shared.Models
{
    public class CustomerGrade
    {
        [Key]
        public int Id { get; set; }

        // 🔹 كود العميل (مفتاح تعريف العميل داخل الملف)
        [MaxLength(50)]
        public string CustomerCode { get; set; }

        // 🧩 المفتاح الأجنبي لربط الدرجة بالعميل
        public int CustomerID { get; set; }  // ← أضف هذا السطر


        // 🔹 رقم الـ Pool المرتبط
        public int PoolId { get; set; }

        // 🔹 رقم الإصدار (Version) الخاص بعملية الاستيراد
        public int Version { get; set; }

        // 🔹 الدرجة (Grade) من 1 إلى 4 مثلاً
        public int GradeValue { get; set; }

        // 🔹 الشهر/السنة الخاصة بهذه الدرجة (من الأعمدة Jan/15 مثلًا)
        public DateTime Month { get; set; }

        // 🔹 تاريخ إنشاء السجل
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
