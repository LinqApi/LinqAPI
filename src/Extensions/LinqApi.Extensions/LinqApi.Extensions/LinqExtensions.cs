using LinqApi.Core;
using LinqApi.Localization.LinqApi.Localization.Extensions;
using LinqApi.Logging;
using LinqApi.Logging.Module;
using LinqApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqApi.Localization.Extensions
{
    /// <summary>
    /// Provides extension methods for registering default LinqApi localization services.
    /// This includes in-memory caching, a database-backed localization repository, and a hosted service
    /// to seed or update localization entries on application startup.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Registers the default localization services used by LinqApi.
        /// This method sets up the localization provider, repository, and a hosted service
        /// to ensure that localization entries are loaded from the database (and cached) at startup.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="configuration">The application configuration for reading connection strings and localization settings.</param>
        /// <returns>The updated IServiceCollection.</returns>
        public static IServiceCollection AddDefaultLinqLocalization(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the repository for localization entities.

            // Register a default in-memory localization provider.
            // This provider will first check the cache and then the repository.
            services.AddSingleton<ILocalizationProvider, DefaultLocalizationProvider>();

            // Register a hosted service that seeds/updates localization entries from the database on startup.
            services.AddHostedService<LocalizationSeederHostedService>();

            // Bind configuration options if necessary.
            services.Configure<LinqLocalizationOptions>(options =>
            {
                options.DefaultCulture = "tr-TR";
                options.EnableCaching = true;
            });
            return services;
        }

        /// <summary>
        /// Provides extension methods for registering repository services based on a DbContext.
        /// </summary>
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

        public static IServiceCollection AddRepositoriesForDbContext<TDbContext>(
    this IServiceCollection services)
    where TDbContext : DbContext
        {
            var dbContextType = typeof(TDbContext);

            // DbSet<SomeEntity> -> SomeEntity
            var dbSetProperties = dbContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsGenericType
                            && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

            foreach (var prop in dbSetProperties)
            {
                var entityType = prop.PropertyType.GetGenericArguments()[0];

                // 'BaseEntity<TId>' olup olmadığını bulalım
                if (TryGetBaseEntityIdType(entityType, out var idType))
                {
                    // ILinqRepository<TEntity, TId>
                    var repoInterface = typeof(ILinqRepository<,>).MakeGenericType(entityType, idType);

                    // LinqRepository<TDbContext, TEntity, TId>
                    var repoImplementation = typeof(LinqRepository<,,>).MakeGenericType(typeof(TDbContext), entityType, idType);

                    services.AddScoped(repoInterface, repoImplementation);
                }
            }

            return services;
        }

        private static readonly ConcurrentDictionary<Type, List<(Type entityType, Type idType)>> _repositoryTypeCache = new();

    //    public static IServiceCollection AddLazyRepositoriesForDbContext<TDbContext>(this IServiceCollection services)
    //where TDbContext : DbContext
    //    {
    //        // ... Reflection ve cache kısmı aynı kalabilir ...
    //        var dbContextType = typeof(TDbContext);

    //        // Bu cache'i burada tutmak yerine reflection sonucunu cache'lemek daha doğru.
    //        if (!_repositoryTypeCache.TryGetValue(dbContextType, out var repoInfoList))
    //        {
    //            // ... reflection ile repoInfoList'i doldurma kodun burada ...
    //            // Bu kısım doğru.
    //        }

    //        // Her bir bulunan repository için lazy registration yapalım.
    //        foreach (var (entityType, idType) in repoInfoList)
    //        {
    //            var repoInterface = typeof(ILinqRepository<,>).MakeGenericType(entityType, idType);
    //            var lazyRepoType = typeof(Lazy<>).MakeGenericType(repoInterface);

    //            // ÖNCEKİ YANLIŞ KOD:
    //            // services.AddScoped(lazyRepoType, sp => { ... });

    //            services.AddScoped(lazyRepoType, sp =>
    //            {
    //                // Bu, Lazy<T>'nin .Value çağrıldığında çalıştıracağı fabrika metodudur.
    //                // Bu içteki lambda, dışarıdaki 'sp' değişkenini "yakalar" (closure).
    //                Func<object> valueFactory = () => sp.GetRequiredService(repoInterface);

    //                // Lazy<T>'yi, bir instance ile değil, fabrika metodu ile oluşturuyoruz.
    //                // Activator, Lazy<T>'nin (Func<T> valueFactory) constructor'ını çağıracak.
    //                return Activator.CreateInstance(lazyRepoType, valueFactory)!;
    //            });
    //        }

    //        return services;
    //    }



        /// <summary>
        /// Sınıf hiyerarşisi içinde BaseEntity<TId> var mı diye arar, bulursa out param ile TId tipini döndürür.
        /// </summary>
        private static bool TryGetBaseEntityIdType(Type type, out Type idType)
        {
            idType = null;
            var current = type;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType &&
                    current.GetGenericTypeDefinition() == typeof(BaseEntity<>))
                {
                    idType = current.GetGenericArguments()[0];
                    return true;
                }
                current = current.BaseType;
            }
            return false;
        }

    }

    public static class ModuleServiceCollectionExtensions
    {
        /// <summary>
        /// CompositeDbContext + Modülleri bir arada register eder.
        /// </summary>
        public static IServiceCollection AddModularDbContext(
            this IServiceCollection services,
            string connectionString,
            IConfiguration configuration,
            Action<ModuleRegistry> configure,
            Action<IServiceProvider, DbContextOptionsBuilder>? configureOptions = null)
        {
            // 1) Module registry oluştur
            var registry = new ModuleRegistry();
            configure(registry);

            // 2) Her modülün kendi servislerini register et
            foreach (var module in registry.Modules)
                module.RegisterServices(services, configuration);

            // 3) Add DbContext<CompositeDbContext>
            services.AddDbContext<CompositeDbContext>(opts =>
                opts.UseSqlServer(connectionString));

            // 4) Modülleri CompositeDbContext’e inject edecek
            services.AddSingleton<IEnumerable<IDbContextModule>>(sp =>
                registry.Modules);

            return services;
        }


        public static IServiceCollection AddModularDbContext<T>(
    this IServiceCollection services,
    IConfiguration configuration,
    string connectionString,
    Action<ModuleRegistry> configureModules,
    Action<IServiceProvider, DbContextOptionsBuilder>? configureOptions = null)
            where T : CompositeDbContext
        {
            // 1) Module registry oluştur
            var registry = new ModuleRegistry();
            configureModules(registry);


            // 2) Her modülün kendi servislerini register et
            foreach (var module in registry.Modules)
                module.RegisterServices(services, configuration);

            // 3) Add DbContext<CompositeDbContext> with extra config
            services.AddDbContext<T>((sp, opts) =>
            {
                opts.UseSqlServer(connectionString);
                // composite db içinde kullanmak üzere serviceProvider'ı devir
                
                configureOptions?.Invoke(sp, opts);
            });

            // 4) Modülleri inject et
            services.AddScoped<IEnumerable<IDbContextModule>>(_ => registry.Modules);

            return services;
        }
    }



}