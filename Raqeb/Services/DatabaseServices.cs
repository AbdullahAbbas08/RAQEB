using Raqeb.DAL.databaseContext;
using Raqeb.DoL.Enums;
using Raqeb.Shared.Models;

namespace Raqeb.Services
{
    public static partial class ServicesRegistration
    {
        public static void AddDatabaseContext(this IServiceCollection services, string connectionString)
        {

            services.AddEntityFrameworkSqlServer().AddDbContext<DatabaseContext>(options =>
            {
                options.UseLazyLoadingProxies(false)
                .UseSqlServer(connectionString, serverDbContextOptionsBuilder =>
                {
                    int minutes = (int)TimeSpan.FromMinutes(3).TotalSeconds;
                    serverDbContextOptionsBuilder.CommandTimeout(minutes);
                    serverDbContextOptionsBuilder.EnableRetryOnFailure();
                });
            });
        }

        public static void DatabaseMigration(this IServiceCollection services)
        {
            //IServiceProvider serviceProvider = services.BuildServiceProvider();
        }

        public static async void DatabaseInitialData(this IServiceCollection services)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IUnitOfWork uow = serviceProvider.GetService<IUnitOfWork>();

            #region Countries Initial
            //string path = $"{uow.ContentRootPath}wwwroot\\countries.json";
            //var jsonContent = File.ReadAllText(path);
            //List<countryFromFile> data = System.Text.Json.JsonSerializer.Deserialize<List<countryFromFile>>(jsonContent);

            //if (!uow.DbContext.Countries.Any())
            //{
            //    foreach (var item in data)
            //    {
            //        uow.DbContext.Countries.Add(new Country { Title = item.name, Code = item.code });
            //    }
            //    uow.SaveChanges();
            //}


            #endregion

         

        }
    }
}
