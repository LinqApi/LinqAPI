using LinqApi.Localization.LinqApi.Localization;
using Microsoft.EntityFrameworkCore;

namespace LinqApi.Localization
{
    public class LinqLocalizationDbContext : DbContext, ILinqLocalizationDbContextAdapter
    {
        protected internal readonly string _schema;

        public LinqLocalizationDbContext(DbContextOptions<LinqLocalizationDbContext> options, string schema)
            : base(options)
        {
            _schema = schema;
        }

        public DbSet<LinqLocalizationEntity> Localizations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyLocalizationModel(this._schema);
            base.OnModelCreating(modelBuilder);
        }
    }

    /// <summary>
    /// Localization DbContext için gereken DbSet ve SaveChanges operasyonlarını expose eden adapter arayüzü.
    /// </summary>
    public interface ILinqLocalizationDbContextAdapter
    {
        DbSet<LinqLocalizationEntity> Localizations { get; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public static class ModelBuilderExtensions
    {
        public static void ApplyLocalizationModel(this ModelBuilder mb, string schema)
        {
            mb.HasDefaultSchema(schema);
            mb.Entity<LinqLocalizationEntity>()
              .ToTable("Localizations");
        }
    }


}
