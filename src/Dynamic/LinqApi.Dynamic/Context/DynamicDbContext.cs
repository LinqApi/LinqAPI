using LinqApi.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace LinqApi.Dynamic.Context
{
    public class DynamicDbContext : DbContext
    {
        private readonly ConcurrentDictionary<string, Type> _dynamicEntities;
        private readonly ConcurrentDictionary<string, string> _primaryKeyMappings;
        private readonly Dictionary<string, Dictionary<string, ColumnDefinition>> _columnSchemas;

        public DynamicDbContext(
            DbContextOptions options,
            ConcurrentDictionary<string, Type> dynamicEntities,
            ConcurrentDictionary<string, string> primaryKeyMappings,
            Dictionary<string, Dictionary<string, ColumnDefinition>> columnSchemas)
            : base(options)
        {
            _dynamicEntities = dynamicEntities;
            _primaryKeyMappings = primaryKeyMappings;
            _columnSchemas = columnSchemas;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in _dynamicEntities)
            {

                var entityKey = entity.Key; // "schema.table"
                var entityType = entity.Value;
                var builder = modelBuilder.Entity(entityType);

                // Schema Normalizasyonu (dbo için tablo adı tek başına kullanılmalı)
                var parts = entityKey.Split('.');
                string schema;
                string table;
                string normalizedSchema;

                string displayName = string.Empty;

                if (parts.Length == 2)
                {
                    schema = parts[0].ToLower();
                    table = parts[1];
                    displayName = schema == "dbo" || string.IsNullOrWhiteSpace(schema)
                       ? table
                    : $"{schema}.{table}";
                }
                else
                {
                    schema = "dbo";
                    table = parts[0];
                    displayName = $"dbo.{table}";
                }
                normalizedSchema = schema.Equals("dbo", StringComparison.OrdinalIgnoreCase) ? "" : schema;
                builder.ToTable(table, schema);

                // Primary Key Tanımlama
                if (_primaryKeyMappings.TryGetValue(entityKey, out string primaryKeyColumnName))
                {
                    builder.HasKey("Id");
                    builder.Property("Id").HasColumnName(primaryKeyColumnName);
                }

                
            }
        }

    }

}
