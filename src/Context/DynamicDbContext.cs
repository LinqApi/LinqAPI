namespace LinqApi.Context
{
    using global::LinqApi.Model;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    namespace LinqApi.Context
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

                    // Tablo ve şema bilgisi ayarlanıyor.
                    var parts = entityKey.Split('.');
                    string schema = parts[0];
                    string table = parts[1];
                    builder.ToTable(table, schema);

                    // Primary key mapping (örneğin, dinamik entity’de "Id" property’si PK olarak işaretleniyor)
                    if (_primaryKeyMappings.TryGetValue(entityKey, out string primaryKeyColumnName))
                    {
                        builder.HasKey("Id");
                        builder.Property("Id").HasColumnName(primaryKeyColumnName);
                    }

                    // Kolon metadata'sını alıyoruz (ColumnDefinition içeren dictionary)
                    if (_columnSchemas.TryGetValue(entityKey, out var columnDefs))
                    {
                        foreach (var property in builder.Metadata.GetProperties())
                        {
                            // Eğer property adının karşılığı columnDefs'te varsa
                            if (columnDefs.TryGetValue(property.Name, out var colDef))
                            {
                                // Örnek olarak; DateTime, int, decimal ve string için ayarlamalar:
                                if (colDef.DotNetType == typeof(DateTime) || colDef.DotNetType == typeof(DateTime?))
                                {
                                    // Örneğin SQL Server’da datetime kullanmak istiyorsak:
                                    builder.Property(property.Name).HasColumnType("datetime");
                                }
                                else if (colDef.DotNetType == typeof(int) || colDef.DotNetType == typeof(int?))
                                {
                                    builder.Property(property.Name).HasColumnType("int");
                                }
                                else if (colDef.DotNetType == typeof(decimal) || colDef.DotNetType == typeof(decimal?))
                                {
                                    if (colDef.NumericPrecision.HasValue && colDef.NumericScale.HasValue)
                                    {
                                        builder.Property(property.Name)
                                            .HasColumnType($"decimal({colDef.NumericPrecision.Value},{colDef.NumericScale.Value})");
                                    }
                                    else
                                    {
                                        builder.Property(property.Name).HasColumnType("decimal(18,2)");
                                    }
                                }
                                else if (colDef.DotNetType == typeof(string))
                                {
                                    if (colDef.MaxLength.HasValue && colDef.MaxLength.Value > 0)
                                    {
                                        builder.Property(property.Name)
                                            .HasColumnType($"nvarchar({colDef.MaxLength.Value})")
                                            .HasMaxLength(colDef.MaxLength.Value);
                                    }
                                    else
                                    {
                                        builder.Property(property.Name).HasColumnType("nvarchar(max)");
                                    }
                                }
                                // Diğer tipler için ek ayarlamalar yapılabilir.
                            }
                        }
                    }
                }
            }
        }
    }

}
