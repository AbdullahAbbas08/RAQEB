namespace Raqeb.Shared.ViewModels.Responses
{
    /// <summary>
    /// 🔹 نموذج استجابة عام لأي API داخل النظام
    /// يحتوي على معلومات النجاح أو الفشل + الرسالة + البيانات إن وُجدت
    /// </summary>
    public class ApiResponse<T>
    {
        // 🔹 هل العملية نجحت أم لا
        public bool Success { get; set; }

        // 🔹 الرسالة التي تُعرض للمستخدم
        public string Message { get; set; }

        // 🔹 بيانات إضافية في حالة النجاح
        public T? Data { get; set; }

        // 🔹 تفاصيل الخطأ (في حالة الفشل)
        public string? ErrorDetails { get; set; }

        // ✅ مُنشئ عام
        public ApiResponse(bool success, string message, T? data = default, string? errorDetails = null)
        {
            Success = success;
            Message = message;
            Data = data;
            ErrorDetails = errorDetails;
        }

        // ✅ دالة مختصرة لإنشاء استجابة ناجحة
        public static ApiResponse<T> SuccessResponse(string message, T? data = default)
        {
            return new ApiResponse<T>(true, message, data);
        }

        // ✅ دالة مختصرة لإنشاء استجابة فاشلة
        public static ApiResponse<T> FailResponse(string message, string? errorDetails = null)
        {
            return new ApiResponse<T>(false, message, default, errorDetails);
        }
    }
}
