using LinqApi.Model;
using LinqApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LinqApi.Extensions
{
    /// <summary>
    /// Provides extension methods for registering repositories from the given assembly,
    /// based on entities derived from <see cref="BaseEntity{TId}"/>.
    /// </summary>
    public static class RepositoryRegistrationExtensions
    {
        /// <summary>
        /// Scans the assembly of the specified DbContext for all non-abstract types derived from <see cref="BaseEntity{TId}"/>
        /// and registers a scoped repository for each found entity.
        /// </summary>
        /// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the repositories to.</param>
        /// <returns>The service collection with repository services registered.</returns>
        public static IServiceCollection AddRepositoriesFromAssembly<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            // Get the assembly where the DbContext is defined.
            var assembly = typeof(TDbContext).Assembly;

            // Retrieve all types from the assembly that are non-abstract and inherit from BaseEntity<>
            var entityTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && IsSubclassOfRawGeneric(typeof(BaseEntity<>), t));

            foreach (var entityType in entityTypes)
            {
                // Retrieve the identifier type from the entity's base type.
                var idType = GetEntityIdType(entityType);

                // Build the repository interface type: ILinqRepository<TEntity, TId>
                var repoInterface = typeof(ILinqRepository<,>).MakeGenericType(entityType, idType);
                // Build the repository implementation type: LinqRepository<TDbContext, TEntity, TId>
                var repoImplementation = typeof(LinqRepository<,,>).MakeGenericType(typeof(TDbContext), entityType, idType);

                services.AddScoped(repoInterface, repoImplementation);
            }

            return services;
        }

        /// <summary>
        /// Registers a repository service for a specific entity type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, which must derive from <see cref="BaseEntity{TId}"/>.</typeparam>
        /// <typeparam name="TId">The type of the entity identifier.</typeparam>
        /// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the repository to.</param>
        /// <returns>The service collection with the repository registered.</returns>
        public static IServiceCollection AddRepository<TEntity, TId, TDbContext>(this IServiceCollection services)
            where TEntity : BaseEntity<TId>
            where TDbContext : DbContext
        {
            services.AddScoped<ILinqRepository<TEntity, TId>, LinqRepository<TDbContext, TEntity, TId>>();
            return services;
        }

        /// <summary>
        /// Checks whether the given type is derived from the specified generic type.
        /// This method traverses the inheritance hierarchy and supports nested types.
        /// </summary>
        /// <param name="generic">The generic type definition to check against (e.g., typeof(BaseEntity&lt;&gt;)).</param>
        /// <param name="toCheck">The type to check.</param>
        /// <returns><c>true</c> if <paramref name="toCheck"/> is derived from the generic type; otherwise, <c>false</c>.</returns>
        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var current = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (current == generic)
                {
                    return true;
                }
                if (toCheck.IsNested && toCheck.DeclaringType != null && IsSubclassOfRawGeneric(generic, toCheck.DeclaringType))
                    return true;
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the identifier type from the entity's base type.
        /// Assumes the entity is derived from <see cref="BaseEntity{TId}"/> and returns the first generic argument.
        /// Defaults to <see cref="int"/> if not found.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The type of the entity's identifier.</returns>
        private static Type GetEntityIdType(Type entityType)
        {
            // Check if the base type is generic.
            if (entityType.BaseType?.IsGenericType == true)
            {
                return entityType.BaseType.GetGenericArguments().First();
            }
            return typeof(int);
        }
    }
}