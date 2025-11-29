using AutoMapper;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Raqeb.AutoMapper;

namespace Raqeb.Services
{
    public static partial class ServicesRegistration
    {
        public static void RegisterAppReuiredServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.RegisterServicesConfiguration();
            builder.Services.AddDistributedMemoryCache();
            // Default IdleTimeout 20 Minutes
            builder.Services.AddSession(opt =>
            {
                opt.Cookie.IsEssential = true;
            });
            builder.Services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
            });

            builder.Services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = long.MaxValue;
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartHeadersLengthLimit = int.MaxValue;
            });


            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
            });

            builder.Services.AddAutoMapper(typeof(ProfileMapping));
            builder.Services.AddMvc();
            builder.Services.ConfigureSwagger();
            builder.Services.AddDatabaseContext(builder.Configuration.GetConnectionString("DefaultConnection"));
            builder.Services.Configure<AppSettings>(options => builder.Configuration.Bind(options));
            bool Hangifier = int.Parse(builder.Configuration.GetSection("Hangifier")["Enable"]) == 1 ? true : false;
            builder.Services.ConfigureHangfire(builder.Configuration.GetConnectionString("DefaultConnection"), Hangifier);
            builder.Services.RunBackgroundService(Hangifier);
            builder.Services.DatabaseMigration();
            builder.Services.DatabaseInitialData();
            builder.Services.AddMemoryCache();
            builder.Services.AddHttpClient();
            //builder.Services.AddHangfireServer(options =>{ options.Queues = HangfireHelper.GetQueues(); });
            builder.Services.ConfigureAuthentication(builder.Configuration);
            builder.Services.AddAuthorizationPolicies();
            builder.Services.AddCorsFromConfiguration(builder.Configuration);
            builder.Services.AddSpaStaticFiles(configuration => { configuration.RootPath = "wwwroot"; });
        }
    }
}
