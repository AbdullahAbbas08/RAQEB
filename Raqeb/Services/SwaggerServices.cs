using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Raqeb.Services
{
    public static partial class ServicesRegistration
    {
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(action =>
            {
                action.SwaggerDoc("v1", new OpenApiInfo { Title = "Raqeb WebApi", Version = "v1" });
                action.MapType<FileContentResult>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "file" });
                action.MapType<object>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "any" });
                action.MapType<JToken>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "any" });
                
            });

            services.AddSwaggerGen();

            services.AddSwaggerDocument();

            services.AddSwaggerGenNewtonsoftSupport(); // explicit opt-in
        }

    }


}
