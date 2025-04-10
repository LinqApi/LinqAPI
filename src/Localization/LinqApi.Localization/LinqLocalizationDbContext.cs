using LinqApi.Localization.LinqApi.Localization;
using Microsoft.EntityFrameworkCore;

namespace LinqApi.Localization
{
    public class LinqLocalizationDbContext : DbContext
    {
        private readonly string _schema;

        public LinqLocalizationDbContext(DbContextOptions<LinqLocalizationDbContext> options, string schema)
            : base(options)
        {
            _schema = schema;
        }

        public DbSet<Culture> Cultures { get; set; }
        public DbSet<HomePage> HomePages { get; set; }
        public DbSet<HomePageLocalization> HomePageLocalizations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Culture yapılandırması
            modelBuilder.Entity<Culture>(entity =>
            {
                entity.ToTable("Cultures", _schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
                entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();

                // Örnek kültür verileri
                entity.HasData(
                    new Culture { Id = 1, Code = "tr-TR", DisplayName = "Türkçe" },
                    new Culture { Id = 2, Code = "en-US", DisplayName = "English" },
                    new Culture { Id = 3, Code = "de-DE", DisplayName = "Deutsch" }
                );
            });

            // HomePage yapılandırması
            modelBuilder.Entity<HomePage>(entity =>
            {
                entity.ToTable("HomePages", _schema);
                entity.HasKey(e => e.Id);
                // Diğer HomePage alanları için konfigürasyon ekleyebilirsiniz.
            });

            // HomePageLocalization yapılandırması
            modelBuilder.Entity<HomePageLocalization>(entity =>
            {
                entity.ToTable("HomePageLocalizations", _schema);
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                // SEO ve view için konfigüre edilen alanlar
                entity.Property(e => e.Title).HasMaxLength(250);
                entity.Property(e => e.MetaDescription).HasMaxLength(300);
                entity.Property(e => e.MetaKeywords).HasMaxLength(250);

                // Culture ilişkisi
                entity.HasOne(e => e.Culture)
                      .WithMany() // İsterseniz Culture sınıfına ilgili koleksiyon ekleyebilirsiniz.
                      .HasForeignKey(e => e.CultureId)
                      .OnDelete(DeleteBehavior.Restrict);

                // HomePage ile ilişkisel bağlantı
                entity.HasOne(e => e.HomePage)
                      .WithMany(h => h.Localizations)
                      .HasForeignKey(e => e.HomePageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
