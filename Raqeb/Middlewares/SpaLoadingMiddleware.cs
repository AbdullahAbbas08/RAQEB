namespace Raqeb.Middlewares.Middlewares
{
    public class SpaLoadingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        public SpaLoadingMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // هنسمح بس في وضع الـ Development
            if (_env.IsDevelopment())
            {
                var client = new HttpClient();

                try
                {
                    var response = await client.GetAsync("http://localhost:4200", HttpCompletionOption.ResponseHeadersRead);

                    if (response.IsSuccessStatusCode)
                    {
                        // Vite شغال → كمل
                        await _next(context);
                        return;
                    }
                }
                catch
                {
                    // تجاهل الاستثناء ونعرض صفحة Loader
                }

                // لسه Vite مش شغال → نعرض loader.html
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "SpaLoader", "loader.html"));

            }
            else
            {
                // في Production كمل عادي
                await _next(context);
            }
        }
    }


    public static class SpaLoadingMiddlewareExtensions
    {
        public static IApplicationBuilder UseSpaLoadingPage(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SpaLoadingMiddleware>();
        }
    }

}
