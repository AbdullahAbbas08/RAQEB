using Raqeb;
using Raqeb.Middlewares;
using Raqeb.Services;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 9999999999;
});
builder.RegisterAppReuiredServices();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "dist";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<FrontendRunner>();
WebApplication app = builder.Build();
// replace with the actual path to wkhtmltopdf on your system
//app.UseMiddleware<DecryptionMiddleware>();

IConfiguration Configuration = app.Configuration;
IHostingEnvironment env= (IHostingEnvironment)app.Environment;
app.ConfigureHTTPRequestPipeline(env, Configuration);
app.Run();
