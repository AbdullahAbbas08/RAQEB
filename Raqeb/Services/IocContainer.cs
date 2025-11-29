using Raqeb.AutoMapper;
using Raqeb.BL.Repositories;
using Raqeb.BL.Services;

namespace Raqeb.Services
{
    public static partial class ServicesRegistration
    {
        public static void RegisterServicesConfiguration(this IServiceCollection services)
        {
            services.AddTransient<IBackgroundJobClient, BackgroundJobClient>();
            services.AddScoped<HttpContextAccessor>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            services.AddScoped<ISessionServices, SessionServices>();
            services.AddScoped<IMailServices, MailServices>();
            services.AddScoped<IDataProtectRepository, DataProtectRepository>();
            services.AddScoped<ITokenStoreRepository, TokenStoreRepository>();
            //services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();
            services.AddScoped<IEncryptionServices, EncryptionServices>();
            services.AddScoped<ILocalizationRepository, LocalizationRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ILGDCalculatorRepository, LGDCalculatorRepository>();
            services.AddScoped<IPDRepository, PDRepository>();
            services.AddScoped<IEclRepository, EclRepository>();

        }
    }
}
