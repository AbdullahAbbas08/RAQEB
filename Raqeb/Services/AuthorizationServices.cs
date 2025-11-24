

namespace Raqeb.Services
{
    public static partial class ServicesRegistration
    {
        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services
             .AddAuthentication(options =>
             {
                 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                 options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
             })
             .AddJwtBearer(cfg =>
             {
                 cfg.RequireHttpsMetadata = false;
                 cfg.SaveToken = true;
                 cfg.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidIssuer = configuration["BearerTokens:Issuer"], 
                     ValidateIssuer = true, 
                     ValidAudience = configuration["BearerTokens:Audience"], 
                     ValidateAudience = true, 
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["BearerTokens:Key"])),
                     ValidateIssuerSigningKey = true, 
                     ValidateLifetime = true, 
                     ClockSkew = TimeSpan.FromMinutes(5)
                 };
                 cfg.Events = new JwtBearerEvents
                 {
                     OnAuthenticationFailed = context =>
                     {
                         ILogger logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                         logger.LogError("Authentication failed.", context.Exception);
                         return Task.CompletedTask;
                     },
                   
                     OnChallenge = context =>
                     {
                         ILogger logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                         logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);
                         return Task.CompletedTask;
                     },
                     OnMessageReceived = context =>
                     {
                         Microsoft.Extensions.Primitives.StringValues accessToken = context.Request.Query["access_token"];

                         return Task.CompletedTask;
                     }
                 };
             });
        }

        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("HasPermissionPolicy", policy =>
            //        policy.Requirements.Add(new HasPermissionRequirment()));
            //    options.AddPolicy("AuthenticatedOnlyPolicy", policy =>
            //        policy.Requirements.Add(new AuthenticatedOnlyRequirment()));
            //});
        }

    }
}
