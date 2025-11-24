using Swashbuckle = Swashbuckle.AspNetCore.Swagger;
using NSwag = NSwag.AspNetCore;

namespace Raqeb.Middlewares.Middlewares
{
    public static partial class _Pipeline
    {
        public static void Swagger(this WebApplication app)
        {
            app.UseOpenApi();
            app.UseSwaggerUi(options =>
            {
                options.Path = "/swagger"; // المسار الافتراضي لواجهة Swagger
            });


        }
    }
}
 