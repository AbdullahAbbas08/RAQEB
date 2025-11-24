namespace Raqeb.Shared.DTOs
{
    /// <summary>
    /// 🔹 كائن منظم لعرض حالة مهمة الاستيراد (Import Job)
    /// </summary>
    public class JobStatusDTO
    {
        public string JobId { get; set; } = string.Empty;          // رقم الـ Job داخل Hangfire
        public string FileName { get; set; } = string.Empty;       // اسم الملف المرفوع
        public string Status { get; set; } = string.Empty;         // Pending / Processing / Success / Failed
        public string ErrorMessage { get; set; }                  // رسالة الخطأ في حالة الفشل
        public DateTime CreatedAt { get; set; }                    // وقت بدء المهمة
        public DateTime? CompletedAt { get; set; }                 // وقت انتهاء المهمة (إن وجد)
    }
}
