using LinqApi.Model;
using LinqApi.Repository;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class RepositoryRegistrationExtensions
{
    public static IServiceCollection AddRepositoriesFromAssembly<TDbContext>(this IServiceCollection services)
      where TDbContext : DbContext
    {
        var assembly = typeof(TDbContext).Assembly;

        var entityTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && IsSubclassOfRawGeneric(typeof(BaseEntity<>), t));

        foreach (var entityType in entityTypes)
        {
            var idType = entityType.BaseType.GetGenericArguments().First();

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

    private static bool IsSubclassOfRawGeneric(Type generic, Type t)
    {
        while (t != null && t != typeof(object))
        {
            var cur = t.IsGenericType ? t.GetGenericTypeDefinition() : t;
            if (cur == generic)
                return true;
            t = t.BaseType;
        }
        return false;
    }
}
