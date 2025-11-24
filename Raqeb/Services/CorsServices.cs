

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raqeb.BL.Filters;

namespace Raqeb.Services
{
    public static partial class ServicesRegistration
    {
        public static string CorsAllowedOriginsPolicyName => "RaqebAllowOrigins";
        public static void AddCorsFromConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                //options.AddPolicy(CorsAllowedOriginsPolicyName, builder =>
                //{
                //    OriginViewModel[] orgins = configuration.GetSection("Origins").Get<OriginViewModel[]>();

                //    foreach (OriginViewModel org in orgins)
                //    {
                //        if (org.AllowAnyHeader && org.AllowAnyMethod)
                //            builder.WithOrigins(org.HostName).AllowAnyHeader().AllowAnyMethod();
                //        else if (org.AllowAnyHeader && !org.AllowAnyMethod)
                //            builder.WithOrigins(org.HostName).AllowAnyHeader();
                //        else if (!org.AllowAnyHeader && org.AllowAnyMethod)
                //            builder.WithOrigins(org.HostName).AllowAnyMethod();
                //        else
                //            builder.WithOrigins(org.HostName);
                //    }
                //});
                options.AddPolicy("AllowAllHeaders",
                builder =>
                {
                    if (configuration.GetSection("CustomOrigin:IsAllowAnyOrigin").Value == "1")
                    {
                        builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    }

                    else
                    {
                        builder.WithOrigins(configuration.GetSection("CustomOrigin:CustomOriginUrl").Value)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    }
                    //.AllowCredentials();
                });
            });
        }

    }
}
