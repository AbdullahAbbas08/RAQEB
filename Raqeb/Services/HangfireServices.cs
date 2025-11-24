namespace Raqeb.Services
{
    public static partial class ServicesRegistration
    {
        public static void ConfigureHangfire(this IServiceCollection services, string connectionString, bool HangifierState)
        {
            if (HangifierState)
            {
                services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
                services.AddHangfireServer();
            }
        }

        public static void RunBackgroundService(this IServiceCollection services, bool HangifierState)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IUnitOfWork uow = serviceProvider.GetService<IUnitOfWork>();

            IRecurringJobManager recurringJobManager = serviceProvider.GetService<IRecurringJobManager>();
            //if (HangifierState)
            //    RecurringJob.AddOrUpdate(() => uow.ActionTracking.GetSiteActionTrackingExpired(), Cron.Daily());
            //BackgroundJob.Enqueue(() => uow.ActionTracking.GetSiteActionTrackingExpired());

            //recurringJobManager.AddOrUpdate(nameof(backgroundJobsService.HourlyTask_UpdateBranchCurrencyRates), () => backgroundJobsService.HourlyTask_UpdateBranchCurrencyRates(), Cron.Hourly(), TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"));
        }

    }
}
