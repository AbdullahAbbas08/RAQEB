namespace Raqeb.Shared.Models
{
    public class ImportJob
    {
        public int Id { get; set; }
        public string JobId { get; set; } = string.Empty; // رقم الـ Hangfire Job
        public string FileName { get; set; } = string.Empty; // اسم الملف المرفوع
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // وقت الرفع
        public DateTime? CompletedAt { get; set; } // وقت الانتهاء
        public string Status { get; set; } = "Pending"; // Pending / Processing / Success / Failed
        public string? ErrorMessage { get; set; } // في حالة الفشل
        public string Type { get; set; }

    }
}
