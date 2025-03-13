namespace LinqApi.Extensions
{
    using global::LinqApi.Model;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;
    using System.Linq;

    namespace LinqApi.Extensions
    {
        public static class ApiExtensions
        {
            public static IServiceCollection AddStaticLinqApi<TDbContext>(this IServiceCollection services, string areaName)
                where TDbContext : DbContext
            {
                var entityTables = new List<TableInfo>();

                var dbContextType = typeof(TDbContext);
                var dbSetProperties = dbContextType.GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                foreach (var prop in dbSetProperties)
                {
                    var entityType = prop.PropertyType.GetGenericArguments()[0];
                    var idType = entityType.BaseType?.GetGenericArguments().FirstOrDefault() ?? typeof(long);

                    var tableName = entityType.Name;
                    var schemaName = "dbo"; // Default olarak dbo kabul ediyoruz.

                    entityTables.Add(new TableInfo
                    {
                        Key = $"{schemaName}.{tableName}",
                        DisplayName = tableName,
                        Area = areaName,
                        SchemaName = schemaName
                    });
                }

                services.AddSingleton(entityTables);

                // Controllerları dinamik olarak eklemek için FeatureProvider kullanacağız.
                var featureProvider = new StaticLinqApiControllerFeatureProvider(typeof(TDbContext), areaName);
                services.AddSingleton(featureProvider);
                services.AddSingleton<IApplicationFeatureProvider<ControllerFeature>>(featureProvider);

                return services;
            }
        }
    }

}
