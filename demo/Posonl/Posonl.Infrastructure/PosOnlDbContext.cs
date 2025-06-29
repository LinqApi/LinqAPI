using LinqApi.Localization;
using LinqApi.Localization.LinqApi.Localization;
using Microsoft.EntityFrameworkCore;
using Posonl.Domain;

namespace Posonl.Infrastructure
{

    public class PosOnlDbContext : LinqLocalizationDbContext
    {
        private readonly string _schema;

        public PosOnlDbContext(DbContextOptions<LinqLocalizationDbContext> options, string schema)
            : base(options, schema)
        {
            _schema = schema ?? "posonl";
        }

        // Ana entity setleri
        public DbSet<Country> Countries { get; set; }
        public DbSet<CountryGroup> CountryGroups { get; set; }
        public DbSet<PosServiceCategory> PosServiceCategories { get; set; }
        public DbSet<PosService> PosServices { get; set; }
        public DbSet<RatingCategory> RatingCategories { get; set; }
        public DbSet<PosCompany> PosCompanies { get; set; }
        public DbSet<PosCompanyRating> PosCompanyRatings { get; set; }
        public DbSet<PosCommissionRate> PosCommissionRates { get; set; }

        // (Opsiyonel) Localization entity setleri — eklenecekse:
        // public DbSet<PosServiceLocalization> PosServiceLocalizations { get; set; }
        // public DbSet<PosCompanyLocalization> PosCompanyLocalizations { get; set; }
        // … Diğer localizable entity’lerin localization setleri

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Artık SEO/localization alanları LocalizationBase üzerinden yönetildi.
            foreach (var entry in ChangeTracker.Entries<LinqLocalizationEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    var entity = entry.Entity;

                   
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Şema adını kullanarak tablo konfigürasyonları:
            modelBuilder.Entity<Country>().ToTable("Country", schema: _schema);
            modelBuilder.Entity<CountryGroup>().ToTable("CountryGroup", schema: _schema);
            modelBuilder.Entity<PosServiceCategory>().ToTable("PosServiceCategory", schema: _schema);
            modelBuilder.Entity<RatingCategory>().ToTable("RatingCategory", schema: _schema);

            modelBuilder.Entity<PosCompanyRating>().ToTable("PosCompanyRating", schema: _schema)
                .HasOne(r => r.RatingCategory)
                .WithMany()  // Eğer RatingCategory tarafında koleksiyon varsa, .WithMany(c => c.PosCompanyRatings) olarak düzenleyebilirsiniz.
                .HasForeignKey(r => r.RatingCategoryId)
                .OnDelete(DeleteBehavior.NoAction); // Cascade delete devre dışı bırakıldı.
            modelBuilder.Entity<PosCommissionRate>().ToTable("PosCommissionRate", schema: _schema);

            // Normal entity konfigürasyonları:
            modelBuilder.Entity<Country>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Code).HasMaxLength(3).IsRequired();
                entity.Property(e => e.Currency).HasMaxLength(10).IsRequired();
                entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired();
            });

            // Eski discriminator (BaseViewEntity) yapılandırması tamamen kaldırıldı.
            // Önceki discriminator bloklarını kaldırıyoruz, çünkü artık her entity kendi tablosunda yer alıyor
            // ve localization alanları ayrık tablolar üzerinden yönetiliyor.

            // Eğer ana entity’lerin kendi ana slug alanlarına ihtiyaç duyuluyorsa, bunları ilgili localization entity’lerinde yönetebilirsiniz.

            // Bazı alanlar isteğe bağlı olsun:
            modelBuilder.Entity<PosCompany>().Property(e => e.StockTicker).IsRequired(false);

            // PosService yapılandırması:
            modelBuilder.Entity<PosService>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasOne(s => s.PosServiceCategory)
                    .WithMany(c => c.PosServices)
                    .HasForeignKey(s => s.PosServiceCategoryId).IsRequired(false);
            });

            // CountryGroup yapılandırması:
            modelBuilder.Entity<CountryGroup>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            });

            // Seed Data
            var countryGroups = new List<CountryGroup>
        {
            new CountryGroup { Id = 1, Name = "Avrupa" },
            new CountryGroup { Id = 2, Name = "Asya" },
            new CountryGroup { Id = 3, Name = "Afrika" },
            new CountryGroup { Id = 4, Name = "Kuzey Amerika" },
            new CountryGroup { Id = 5, Name = "Güney Amerika" },
            new CountryGroup { Id = 6, Name = "Okyanusya" },
            new CountryGroup { Id = 7, Name = "Orta Doğu" },
            new CountryGroup { Id = 8, Name = "Türk Devletleri" }
        };
            modelBuilder.Entity<CountryGroup>().HasData(countryGroups);

            modelBuilder.Entity<Country>().HasData(PosOnlDbContextHelpers.GetSeedCountries());
            var categories = PosOnlDbContextHelpers.GetSeedPosServiceCategories();
            modelBuilder.Entity<PosServiceCategory>().HasData(categories);

            var serviceData = PosOnlDbContextHelpers.GetPosServiceData();
            Dictionary<string, long> serviceKeyToIdMap = PosOnlDbContextHelpers.SeedPosServicesFromData(modelBuilder, categories, serviceData);

            var companies = PosOnlDbContextHelpers.SeedPosCompanies(modelBuilder);
            modelBuilder.Entity<PosCompany>().HasData(companies);
        }
    }

}