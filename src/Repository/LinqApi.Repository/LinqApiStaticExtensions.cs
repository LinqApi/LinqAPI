using LinqApi.Model;
using LinqApi.Repository;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering repository services based on a DbContext.
/// </summary>
public static class RepositoryServiceCollectionExtensions
{
    /// <summary>
    /// Scans the specified DbContext for all DbSet properties and registers repository services
    /// for each entity type that is derived from <typeparamref name="TEntityBase"/>.
    /// The repository interface and its implementation are registered using the specified identifier type.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
    /// <typeparam name="TEntityBase">The base entity type from which all registered entities must derive.</typeparam>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    /// <param name="services">The service collection to add the repositories to.</param>
    /// <returns>The service collection with the repository services registered.</returns>
    public static IServiceCollection AddRepositoriesForDbContext<TDbContext, TEntityBase, TId>(
        this IServiceCollection services)
        where TDbContext : DbContext
        where TEntityBase : BaseEntity<TId>
    {
        // Get the DbContext type.
        var dbContextType = typeof(TDbContext);

        // Retrieve all public instance properties of type DbSet<T> from the DbContext.
        var dbSetProperties = dbContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        // Loop through each DbSet property.
        foreach (var prop in dbSetProperties)
        {
            // Get the entity type (T) from DbSet<T>.
            var entityType = prop.PropertyType.GetGenericArguments()[0];

            // Only register if the entity type inherits from TEntityBase.
            if (typeof(TEntityBase).IsAssignableFrom(entityType))
            {
                // Construct the repository interface type: ILinqRepository<TEntity, TId>
                var repoInterface = typeof(ILinqRepository<,>).MakeGenericType(entityType, typeof(TId));

                // Construct the repository implementation type: LinqRepository<TDbContext, TEntity, TId>
                var repoImplementation = typeof(LinqRepository<,,>).MakeGenericType(typeof(TDbContext), entityType, typeof(TId));

                // Register the repository as a scoped service.
                services.AddScoped(repoInterface, repoImplementation);
            }
        }

        return services;
    }
}