using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace LinqApi.Context
{
    public class DynamicDbContext : DbContext
    {
        private readonly ConcurrentDictionary<string, Type> _dynamicEntities;
        private readonly ConcurrentDictionary<string, string> _primaryKeyMappings;

        public DynamicDbContext(DbContextOptions<DynamicDbContext> options,
                                ConcurrentDictionary<string, Type> dynamicEntities,
                                ConcurrentDictionary<string, string> primaryKeyMappings)
            : base(options)
        {
            _dynamicEntities = dynamicEntities;
            _primaryKeyMappings = primaryKeyMappings;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in _dynamicEntities)
            {
                var entityType = entity.Value;
                var builder = modelBuilder.Entity(entityType);
                // key format: "schema.table"
                var parts = entity.Key.Split('.');
                string schema = parts[0];
                string table = parts[1];
                // EF’ye tablo ve şema bilgisini veriyoruz
                builder.ToTable(table, schema);

                if (_primaryKeyMappings.TryGetValue(entity.Key, out string primaryKeyColumnName))
                {
                    builder.HasKey("Id");
                    builder.Property("Id").HasColumnName(primaryKeyColumnName);
                }
            }
        }
    }
}
