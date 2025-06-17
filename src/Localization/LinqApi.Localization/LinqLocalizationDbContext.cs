using LinqApi.Localization.LinqApi.Localization;
using Microsoft.EntityFrameworkCore;

namespace LinqApi.Localization
{
    public class LinqLocalizationDbContext : DbContext
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
            base.OnModelCreating(modelBuilder);


        }
    }
}
