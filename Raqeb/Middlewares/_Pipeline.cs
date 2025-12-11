using Raqeb.API.Middlewares;
using Raqeb.Middlewares.Middlewares;
using Raqeb.Services;

namespace Raqeb.Middlewares
{
    public static class _Pipeline
    {
        public static void ConfigureHTTPRequestPipeline(this WebApplication app, Microsoft.Extensions.Hosting.IHostingEnvironment env, IConfiguration Configuration)
        {
            app.MapControllers();
            app.UseSession();
            //app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Angular deep links fallback (Production)
            app.MapFallbackToFile("index.html");

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            else app.UseHsts();
            //app.UseResponseCompression();
            //app.UseSession();
            app.UseFileServer(new FileServerOptions() { EnableDirectoryBrowsing = false });
            app.UseGlobalExceptionHandler();

            app.UseCors(ServicesRegistration.CorsAllowedOriginsPolicyName);
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            if( int.Parse(app.Configuration.GetSection("Hangifier")["Enable"]) == 1 ? true : false)
            app.UseHangfireDashboard("/hangfire");
            app.UseEndpoints(endpoints =>   
            {
                endpoints.MapControllers();
            });
            //if (app.Environment.IsDevelopment())
            app.Swagger();
            app.SinglePageApp(env, Configuration);

        }
    }

}
