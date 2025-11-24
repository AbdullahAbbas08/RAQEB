using Raqeb.Shared.ViewModels.Responses;
using System.Net;

namespace Raqeb.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // استدعاء باقي الـ pipeline
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Unhandled exception: {ex.Message}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var errorResponse = ApiResponse<object>.FailResponse(
                      // 🔹 الرسالة العامة للمستخدم النهائي (ودّية)
                      ex.Message ?? "An unexpected error occurred.",

                      // 🔹 تفاصيل إضافية للمطورين (تشمل كود الخطأ لتسهيل التتبع)
                      context.Response.StatusCode switch
                      {
                          400 => "Error Code: ERR_BAD_REQUEST",
                          401 => "Error Code: ERR_UNAUTHORIZED",
                          404 => "Error Code: ERR_NOT_FOUND",
                          _ => "Error Code: ERR_INTERNAL"
                      }
                  );


            // ✅ استخدم Newtonsoft.Json هنا
            var json = JsonConvert.SerializeObject(errorResponse, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            });

            await context.Response.WriteAsync(json);
        }
    }

    // 🔹 امتداد علشان نستخدمه بسهولة في Program.cs
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
