using LinqApi.Dynamic.Context;
using LinqApi.Dynamic.Controller;
using LinqApi.Dynamic.Helpers;
using LinqApi.Model;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace LinqApi.Dynamic.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddDynamicLinqApi(this IServiceCollection services, string areaName, string connectionString)
        {
            // 1. Tabloları ve şema bilgilerini alıyoruz.
            var tables = DatabaseHelper.GetAllTablesWithSchema(connectionString);
            var dynamicEntities = new ConcurrentDictionary<string, Type>(Environment.ProcessorCount, tables.Count);
            var primaryKeyMappings = new ConcurrentDictionary<string, string>(Environment.ProcessorCount, tables.Count);
            var columnSchemas = DatabaseHelper.GetAllTableSchemas(connectionString);
            var allPrimaryKeys = DatabaseHelper.GetAllPrimaryKeys(connectionString);

            // DI'ya ilgili nesneleri ekleyelim:
            services.AddSingleton(dynamicEntities);
            services.AddSingleton(primaryKeyMappings);
            services.AddSingleton(columnSchemas);

            Parallel.ForEach(tables, tableInfo =>
            {
                var (schema, table) = tableInfo;

                // Schema normalizasyonu (dbo için sadece tablo adı kullanılacak)
                string normalizedSchema = schema.Equals("dbo", StringComparison.OrdinalIgnoreCase) ? "" : schema;
                string key = string.IsNullOrWhiteSpace(normalizedSchema) ? table : $"{normalizedSchema}.{table}";

            if (!columnSchemas.TryGetValue($"{key}", out Dictionary<string, ColumnDefinition> columns))
                    return;
                if (!allPrimaryKeys.TryGetValue($"{key}", out string pkColumn))
                    return;
                if (!columns.TryGetValue(pkColumn, out ColumnDefinition pkColDef))
                    return;
                if (!IsValidPrimaryKey(pkColDef.DotNetType))
                    return;



                var primaryKeyPair = new KeyValuePair<string, Type>(pkColumn, pkColDef.DotNetType);
                var columnsForEntity = columns.ToDictionary(x => x.Key, x => x.Value.DotNetType);
                var entityType = EntityGenerator.GenerateEntity(schema, table, primaryKeyPair, columnsForEntity, table);

                dynamicEntities.TryAdd(key, entityType);
                primaryKeyMappings.TryAdd(key, pkColumn);
            });

            // Yeni konfigürasyonu registry'ye ekleyelim.
            var config = new LinqMsApiConfiguration
            {
                AreaName = areaName,
                ConnectionString = connectionString,
                DynamicEntities = dynamicEntities,
                PrimaryKeyMappings = primaryKeyMappings,
                ColumnSchemas = columnSchemas
            };
            LinqMsApiRegistry.Configurations[areaName] = config;

            // DbContext’i ayrı bir türevi olarak kaydetmek için:
            var dbContextType = DynamicDbContextGenerator.GenerateDbContextType(areaName);
            services.Add(new ServiceDescriptor(
                dbContextType,
                sp =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder();
                    optionsBuilder.UseSqlServer(connectionString);
                    var dynEntities = sp.GetRequiredService<ConcurrentDictionary<string, Type>>();
                    var pkMappings = sp.GetRequiredService<ConcurrentDictionary<string, string>>();
                    var colSchemas = sp.GetRequiredService<Dictionary<string, Dictionary<string, ColumnDefinition>>>();
                    return ActivatorUtilities.CreateInstance(sp, dbContextType, optionsBuilder.Options, dynEntities, pkMappings, colSchemas);
                },
                ServiceLifetime.Scoped));

            // Dinamik controller feature provider'ı, areaName’i kullanacak şekilde oluşturun:
            var featureProvider = new DynamicLinqApiControllerFeatureProvider(dynamicEntities, areaName);
            services.AddSingleton(featureProvider);
            services.AddSingleton<IApplicationFeatureProvider<ControllerFeature>>(featureProvider);

            // Repository’leri, oluşturduğumuz dbContext türevi üzerinden kaydedin:
            DynamicRepositoryHelper.AddRepositories(services, dynamicEntities, dbContextType);

            return services;
        }

        private static bool IsValidPrimaryKey(Type keyType)
        {
            // Örneğin, bigint (long) tipli primary key'ler bazı yapılar tarafından kabul edilmeyebilir.
            // Bu kontrol ile long tipini reddediyoruz.
            if (keyType == typeof(long))
                return false;
            return keyType == typeof(int) ||
                   keyType == typeof(string) ||
                   keyType == typeof(Guid) ||
                   keyType == typeof(DateTime) ||
                   keyType == typeof(short);
        }
    }


}
