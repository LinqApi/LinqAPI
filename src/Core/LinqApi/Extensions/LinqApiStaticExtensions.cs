using LinqApi.Model;
using LinqApi.Repository;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class RepositoryRegistrationExtensions
{
    public static IServiceCollection AddRepositoriesFromAssembly<TDbContext, TEntityBase, TId>(
     this IServiceCollection services,
     params Assembly[] assemblies)
     where TDbContext : DbContext
     where TEntityBase : class
    {
        // Eğer assembly parametresi verilmemişse, TEntityBase'nin tanımlı olduğu assembly'yi kullan.
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { typeof(TEntityBase).Assembly };
        }

        // Verilen tüm assembly'lerden tipleri alıyoruz.
        
        var allTypes = assemblies.SelectMany(a => a.GetTypes());

        // TEntityBase'ten türeyen (assignable) ve soyut olmayan tipleri filtreliyoruz.
        var entityTypes = allTypes.Where(t => !t.IsAbstract && typeof(TEntityBase).IsAssignableFrom(t));

        foreach (var entityType in entityTypes)
        {
            // Örneğin entity'leriniz doğrudan BaseEntity<TId>'den türediği varsayılıyor.
            
            var idType = typeof(TId);

            var repoInterface = typeof(ILinqRepository<,>).MakeGenericType(entityType, idType);
            var repoImplementation = typeof(LinqRepository<,,>).MakeGenericType(typeof(TDbContext), entityType, idType);

            services.AddScoped(repoInterface, repoImplementation);
        }

        return services;
    }
    public static IServiceCollection AddRepository<TEntity, TId, TDbContext>(this IServiceCollection services)
        where TEntity : BaseEntity<TId>
        where TDbContext : DbContext
    {
        services.AddScoped<ILinqRepository<TEntity, TId>, LinqRepository<TDbContext, TEntity, TId>>();
        return services;
    }

    public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }
            // Eğer tür nested ise, declaring type'ı da kontrol et
            if (toCheck.IsNested && toCheck.DeclaringType != null)
            {
                if (IsSubclassOfRawGeneric(generic, toCheck.DeclaringType))
                    return true;
            }
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}
