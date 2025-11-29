using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raqeb.Shared.Models;
using Raqeb.Shared.Models.ECL;
using Raqeb.Shared.Models.Raqeb.Shared.Models;
using Raqeb.Shared.ViewModels.DTOs;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;

namespace Raqeb.DAL.databaseContext
{
    public class DatabaseContext : DbContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public DatabaseContext(DbContextOptions<DatabaseContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            this.httpContextAccessor = httpContextAccessor;
        }


        #region DbSets

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Pool> Pools { get; set; }
        public DbSet<RecoveryRecord> RecoveryRecords { get; set; }
        public DbSet<PoolLGDResult> PoolLGDResults { get; set; }
        public DbSet<ImportJob> ImportJobs { get; set; }
        public DbSet<PDMatrixCell> PDMatrixCells { get; set; }
        public DbSet<PDTransitionCell> PDTransitionCells { get; set; }
        public DbSet<PDAverageCell> PDAverageCells { get; set; }
        public DbSet<PDLongRunCell> PDLongRunCells { get; set; }
        public DbSet<PDObservedRate> PDObservedRates { get; set; }
        public DbSet<CustomerGrade> CustomerGrades { get; set; }
        public DbSet<PDMonthlyTransitionCell> PDMonthlyTransitionCells { get; set; }
        public DbSet<PDYearlyAverageCell> PDYearlyAverageCells { get; set; } 
        public DbSet<PDMonthlyRowStat> PDMonthlyRowStats { get; set; } 
        public DbSet<PDCalibrationResult> PDCalibrationResults  { get; set; } 
        public DbSet<PDLongRunAverage> PDLongRunAverages { get; set; } 
        public DbSet<MacroeconomicInput> MacroeconomicInputs { get; set; } 
        public DbSet<PDMarginalResult> PDMarginalResults { get; set; }

        public DbSet<MacroScenarioValue> MacroScenarioValues { get; set; }
        public DbSet<MacroScenarioIndex> MacroScenarioIndices { get; set; }



        //ECL Summery
        public DbSet<EclCustomerInput> EclCustomers { get; set; }
        public DbSet<EclCcfInput> ECL_CCF { get; set; }
        public DbSet<EclMacroeconomicInput> EclMacroeconomics { get; set; }
        public DbSet<EclSicrMatrixInput> EclSicrMatrixs { get; set; }
        public DbSet<EclCureRateInput> EclCureRates { get; set; }
        public DbSet<EclScoreGrade> EclScoreGrades { get; set; }
        public DbSet<EclDpdBucket> EclDpdBuckets { get; set; }
        public DbSet<EclScenarioWeight> EclScenarioWeights { get; set; }
        public DbSet<EclGradeSummary> EclGradeSummary { get; set; }
        public DbSet<EclStageSummary> EclStageSummary { get; set; }



        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<SystemSetting> SystemSettings { get; set; }
        public virtual DbSet<CustomerSystemSetting> CustomersSystemSetting { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<LanguageLocalization> LanguageLocalizations { get; set; }
        public virtual DbSet<Localization> Localizations { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<Region> Regions { get; set; }

        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Pool>()
          .HasMany(p => p.Customers)
          .WithOne(c => c.Pool)
          .HasForeignKey(c => c.PoolId);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Recoveries)
                .WithOne(r => r.Customer)
                .HasForeignKey(r => r.CustomerId);


            modelBuilder.Entity<UserToken>(b =>
                {
                    b.HasOne(b => b.User)
                    .WithMany(c => c.UserTokens)
                    .HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity<Localization>()
          .HasIndex(l => l.Code)
          .IsUnique();


            #region GroupAudit
            modelBuilder.Entity<Group>(b =>
            {
                b.HasOne(b => b.CreatedByUser)
                .WithMany()
                .HasForeignKey(b => b.CreatedBy).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Group>(b =>
            {
                b.HasOne(b => b.UpdatedByUser)
                .WithMany()
                .HasForeignKey(b => b.UpdatedBy).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Group>(b =>
            {
                b.HasOne(b => b.DeletedByUser)
                .WithMany()
                .HasForeignKey(b => b.DeletedBy).OnDelete(DeleteBehavior.Restrict);
            });

            #endregion




            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            int? UserId = null;
            var claim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var _UserId = EncryptHelper.DecryptString(claim.Value);
                UserId = int.Parse(_UserId);
            }


            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var entity = entry.Entity;
                var createdOnProperty = entity.GetType().GetProperty("CreatedOn");
                var createdByProperty = entity.GetType().GetProperty("CreatedBy");
                if (createdOnProperty != null && createdByProperty != null && entry.State == EntityState.Added)
                {
                    createdByProperty.SetValue(entity, UserId);
                    createdOnProperty.SetValue(entity, DateTime.Now);
                }
                var modifiedOnProperty = entity.GetType().GetProperty("UpdatedOn");
                var modifiedByProperty = entity.GetType().GetProperty("UpdatedBy");
                if (modifiedOnProperty != null && modifiedByProperty != null && entry.State == EntityState.Modified)
                {
                    modifiedOnProperty.SetValue(entity, DateTime.Now);
                    modifiedByProperty.SetValue(entity, UserId);
                }
            }

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            int? userId = null;

            if (httpContextAccessor.HttpContext != null && httpContextAccessor.HttpContext.User != null)
            {
                var claim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null)
                {
                    var decryptedUserId = EncryptHelper.DecryptString(claim.Value);
                    userId = int.Parse(decryptedUserId);
                }
            }

            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var entity = entry.Entity;
                var createdOnProperty = entity.GetType().GetProperty("CreatedOn");
                var createdByProperty = entity.GetType().GetProperty("CreatedBy");

                if (createdOnProperty != null && createdByProperty != null && entry.State == EntityState.Added)
                {
                    createdByProperty.SetValue(entity, userId);
                    createdOnProperty.SetValue(entity, DateTime.Now);
                }

                var modifiedOnProperty = entity.GetType().GetProperty("UpdatedOn");
                var modifiedByProperty = entity.GetType().GetProperty("UpdatedBy");

                if (modifiedOnProperty != null && modifiedByProperty != null && entry.State == EntityState.Modified)
                {
                    modifiedOnProperty.SetValue(entity, DateTime.Now);
                    modifiedByProperty.SetValue(entity, userId);
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SaveHistory()
        {
            string user = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
            User currentUser = Users.FirstOrDefault(u => u.Email == user);

            foreach (EntityEntry entityEntry in ChangeTracker.Entries())
            {
                object obj = entityEntry.Entity;
                Type type = obj.GetType();

                if (entityEntry.State == EntityState.Added)
                {
                    PropertyInfo createdOnPropertyInfo = type.GetProperty("CreatedOn");
                    if (createdOnPropertyInfo != null) createdOnPropertyInfo.SetValue(obj, DateTime.Now);

                    PropertyInfo createdByIdPropertyInfo = type.GetProperty("CreatedById");
                    if (createdByIdPropertyInfo != null)
                    {
                        object createdBy = createdByIdPropertyInfo.GetValue(obj);
                        if (string.IsNullOrWhiteSpace(createdBy?.ToString())) createdByIdPropertyInfo.SetValue(obj, currentUser?.ID);
                    }

                    PropertyInfo ownerIdPropertyInfo = type.GetProperty("OwnerId");
                    if (ownerIdPropertyInfo != null) ownerIdPropertyInfo.SetValue(obj, currentUser?.ID);
                }

                if (entityEntry.State == EntityState.Modified || entityEntry.State == EntityState.Added)
                {
                    PropertyInfo modifiedOnPropertyInfo = type.GetProperty("ModifiedOn");
                    if (modifiedOnPropertyInfo != null) modifiedOnPropertyInfo.SetValue(obj, DateTime.Now);

                    PropertyInfo modifiedByIdPropertyInfo = type.GetProperty("ModifiedById");
                    if (modifiedByIdPropertyInfo != null) modifiedByIdPropertyInfo.SetValue(obj, currentUser?.ID);
                }
            }
        }

    }
}
