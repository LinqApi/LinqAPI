using LinqApi.Localization;
using LinqApi.Localization.LinqApi.Localization;
using LinqApi.Localization.LinqApi.Localization.Extensions;
using Microsoft.EntityFrameworkCore;
using Posonl.Domain;

namespace Posonl.Infrastructure
{

    public class PosOnlDbContext : LinqLocalizationDbContext
    {
        private readonly string _schema;

        public PosOnlDbContext(DbContextOptions<LinqLocalizationDbContext> options, string schema) : base(options, schema)
        {
            _schema = schema ?? "posonl";
        }

        public DbSet<News> News => Set<News>();

        public DbSet<Country> Countries { get; set; }
        public DbSet<CountryGroup> CountryGroups { get; set; }
        public DbSet<PosServiceCategory> PosServiceCategories { get; set; }
        public DbSet<PosService> PosServices { get; set; }
        public DbSet<RatingCategory> RatingCategories { get; set; }
        public DbSet<PosCompany> PosCompanies { get; set; }
        public DbSet<PosCompanyType> PosCompanyTypes { get; set; }
        public DbSet<PosCompanyDescription> PosCompanyDescriptions { get; set; }
        public DbSet<PosCompanyRating> PosCompanyRatings { get; set; }
        public DbSet<PosCommissionRate> PosCommissionRates { get; set; }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseViewEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    var entity = entry.Entity;

                    if (string.IsNullOrWhiteSpace(entity.Slug) && !string.IsNullOrWhiteSpace(entity.Title))
                    {
                        entity.Slug = SlugHelper.SlugifyWithTitle(entity.Title);
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Şema adını kullanarak entity konfigürasyonlarını yap
            modelBuilder.Entity<Country>().ToTable("Country", schema: _schema);
            modelBuilder.Entity<CountryGroup>().ToTable("CountryGroup", schema: _schema);
            //modelBuilder.Entity<PosServiceCategory>().ToTable("PosServiceCategory", schema: _schema);
            //modelBuilder.Entity<RatingCategory>().ToTable("RatingCategory", schema: _schema);
            //modelBuilder.Entity<PosCompanyType>().ToTable("PosCompanyType", schema: _schema);
            //modelBuilder.Entity<PosCompanyDescription>().ToTable("PosCompanyDescription", schema: _schema);
            modelBuilder.Entity<PosCompanyRating>().ToTable("PosCompanyRating", schema: _schema)
     .HasOne(r => r.RatingCategory) // PosCompanyRating içerisindeki RatingCategory navigasyon property’si
     .WithMany() // RatingCategory’nin PosCompanyRatings koleksiyonu varsa
     .HasForeignKey(r => r.RatingCategoryId)
     .OnDelete(DeleteBehavior.NoAction); // Cascade delete devre dışı bırakılıyor
            modelBuilder.Entity<PosCommissionRate>().ToTable("PosCommissionRate", schema: _schema);

            // Map the inheritance hierarchy (BaseViewEntity and its derived types) to a single table.
            //modelBuilder.Entity<BaseViewEntity>().ToTable("BaseViewEntity", schema: _schema);
            //modelBuilder.Entity<News>().ToTable("BaseViewEntity", schema: _schema);
            //modelBuilder.Entity<PosService>().ToTable("BaseViewEntity", schema: _schema);
            //modelBuilder.Entity<PosCompany>().ToTable("BaseViewEntity", schema: _schema);

            // Diğer entity konfigürasyonları...

            modelBuilder.Entity<Country>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Code).HasMaxLength(3).IsRequired();
                entity.Property(e => e.Currency).HasMaxLength(10).IsRequired();
                entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired();
            });

            // Discriminator yapılandırması:
            modelBuilder.Entity<BaseViewEntity>(b =>
            {
                b.HasDiscriminator<string>("ViewType")
                    .HasValue<News>("news")
                    .HasValue<PosCompany>("poscompany")
                    .HasValue<PosService>("posservice")
                    .HasValue<PosCompanyType>("poscompanytype")  // Eğer varsa diğer türevler için
                .HasValue<RatingCategory>("ratingcategory")  // Eğer varsa diğer türevler için
                .HasValue<PosCompanyDescription>("posCompanyDescription");
                b.Property("ViewType").HasMaxLength(50);
                b.HasIndex("Slug").IsUnique();
            });

            modelBuilder.Entity<News>().HasIndex(n => n.Slug).IsUnique();
            modelBuilder.Entity<PosService>().HasIndex(p => p.Slug).IsUnique();
            modelBuilder.Entity<PosCompany>().HasIndex(p => p.Slug).IsUnique();

            modelBuilder.Entity<PosCompany>().Property(e => e.StockTicker).IsRequired(false);
            modelBuilder.Entity<PosCompany>().Property(e => e.MetaKeywords).IsRequired(false);
            modelBuilder.Entity<News>().Property(e => e.MetaKeywords).IsRequired(false);
            modelBuilder.Entity<PosService>().Property(e => e.MetaKeywords).IsRequired(false);

            modelBuilder.Entity<PosService>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasOne(s => s.PosServiceCategory)
                    .WithMany(c => c.PosServices)
                    .HasForeignKey(s => s.PosServiceCategoryId).IsRequired(false);
            });

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