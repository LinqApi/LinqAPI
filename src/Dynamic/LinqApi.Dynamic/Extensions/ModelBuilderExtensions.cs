using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqApi.Dynamic.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void ConfigureForeignKeyNavigationProperties(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Entity içerisindeki tüm properties'i gezelim
                foreach (var property in entityType.GetProperties())
                {
                    if (property.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        // Navigation property adı: "UserId" => "User" gibi
                        var navigationName = property.Name.Substring(0, property.Name.Length - 2);

                        // İlgili navigation property daha önce eklenmemişse
                        // (EF Core'da navigation property'leri runtime'da eklemek doğrudan desteklenmez,
                        // fakat bu extension, ilişkilendirilebilecek entity'leri belirleyip,
                        // config için loglama veya uyarı mekanizması oluşturabilir.)
                        var alreadyExists = entityType.GetNavigations().Any(n => n.Name.Equals(navigationName, StringComparison.OrdinalIgnoreCase));
                        if (!alreadyExists)
                        {
                            // İlgili entity tipi, modelde navigation adıyla eşleşen bir entity arayalım.
                            var relatedEntityType = modelBuilder.Model.GetEntityTypes()
                                .FirstOrDefault(e => e.ClrType.Name.Equals(navigationName, StringComparison.OrdinalIgnoreCase));
                            if (relatedEntityType != null)
                            {
                                // Burada fluent API ile ilişkiyi belirtmek mümkün, ancak
                                // navigation property eklemek, genellikle entity sınıfı düzeyinde yapılması gerekir.
                                // Yine de, foreign key ilişkisini tanımlamak için örnek bir kod:
                                modelBuilder.Entity(entityType.ClrType)
                                    .HasOne(relatedEntityType.ClrType)
                                    .WithMany()
                                    .HasForeignKey(property.Name);
                            }
                        }
                    }
                }
            }
        }
    }
}
