//using LinqApi.Helpers;
//using LinqApi.Model;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace LinqApi.Extensions
//{
//    public static IServiceCollection AddLinqApiStatic<TDbContext>(this IServiceCollection services, string areaName, string connectionString)
//        where TDbContext : DbContext
//    {
//        // DbContext'i connection string ile register ediyoruz.
//        services.AddDbContext<TDbContext>(options =>
//            options.UseSqlServer(connectionString));

//        // Statik senaryo için LinqMsApiConfiguration oluşturuyoruz.
//        var config = new LinqMsApiConfiguration
//        {
//            AreaName = areaName,
//            ConnectionString = connectionString,
//            // Statik kullanımda dynamic entity sözlüğü kullanılmayacak, boş bir dictionary oluşturuyoruz.
//            DynamicEntities = new ConcurrentDictionary<string, Type>(),
//            PrimaryKeyMappings = new ConcurrentDictionary<string, string>(),
//            ColumnSchemas = new Dictionary<string, Dictionary<string, ColumnDefinition>>()
//        };
//        LinqMsApiRegistry.Configurations[areaName] = config;

        

//        return services;
//    }
//}
