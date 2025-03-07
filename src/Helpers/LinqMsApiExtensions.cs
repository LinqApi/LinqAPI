using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Concurrent;
using System.Linq;
using System;
using LinqApi.Context;

namespace LinqApi.Helpers
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddLinqMsApi(this IServiceCollection services, string connectionString)
        {
            // Tüm tablo (schema, table) bilgilerini toplu sorguyla alalım
            var tables = DatabaseHelper.GetAllTablesWithSchema(connectionString);
            // ConcurrentDictionary ile paralel eklemelerde lock kullanımını minimize ediyoruz
            var dynamicEntities = new ConcurrentDictionary<string, Type>(Environment.ProcessorCount, tables.Count);
            var primaryKeyMappings = new ConcurrentDictionary<string, string>(Environment.ProcessorCount, tables.Count);

            // Toplu sorgular: tüm tablo şema bilgileri ve primary key bilgilerini alalım
            var allTableSchemas = DatabaseHelper.GetAllTableSchemas(connectionString);
            var allPrimaryKeys = DatabaseHelper.GetAllPrimaryKeys(connectionString);

            Parallel.ForEach(tables, tableInfo =>
            {
                var (schema, table) = tableInfo;
                string key = $"{schema}.{table}";
                if (!allTableSchemas.TryGetValue(key, out Dictionary<string, Type> columns))
                    return;
                if (!allPrimaryKeys.TryGetValue(key, out string pkColumn))
                    return;
                if (!columns.TryGetValue(pkColumn, out Type pkType))
                    return;
                if (!IsValidPrimaryKey(pkType))
                    return;

                var primaryKeyPair = new KeyValuePair<string, Type>(pkColumn, pkType);
                // Eğer schema "dbo" ise entity adı sadece tablo adı, değilse "schema_tablename"
                string entityName = (schema.ToLower() == "dbo" ? table : $"{schema}_{table}");
                var entityType = EntityGenerator.GenerateEntity(schema, table, primaryKeyPair, columns, entityName);
                dynamicEntities.TryAdd(key, entityType);
                primaryKeyMappings.TryAdd(key, pkColumn);
            });

            services.AddSingleton(dynamicEntities);
            services.AddSingleton(primaryKeyMappings);

            services.AddDbContext<DynamicDbContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString);
            });

            var featureProvider = new DynamicLinqmsControllerFeatureProvider(dynamicEntities);
            services.AddSingleton(featureProvider);
            services.AddSingleton<IApplicationFeatureProvider<ControllerFeature>>(featureProvider);

            RepositoryHelper.AddRepositories(services, dynamicEntities);

            return services;
        }

        private static bool IsValidPrimaryKey(Type keyType)
        {
            return keyType == typeof(int) ||
                   keyType == typeof(long) ||
                   keyType == typeof(string) ||
                   keyType == typeof(Guid) ||
                   keyType == typeof(DateTime) ||
                   keyType == typeof(short);
        }
    }
}
