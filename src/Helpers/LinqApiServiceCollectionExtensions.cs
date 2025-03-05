using AutoMapper;
using LinqApi.Model;
using LinqApi.Repository;
using LinqApi.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LinqApi.Helpers
{
    public static class LinqApiServiceCollectionExtensions
    {
        public static IServiceCollection AddLinqApi<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            // AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoLinqMappingProfile>();
            });

            services.AddSingleton<IMapper>(mapperConfig.CreateMapper());
            services.AddScoped(typeof(ILinqService<,,>), typeof(LinqService<,,>));


            // BaseEntity türetilmiş entity'leri bul
            var entityTypes = typeof(TDbContext).Assembly
                .GetTypes()
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(BaseEntity<>))
                .ToList();

            foreach (var entityType in entityTypes)
            {
                var idType = entityType.BaseType.GetGenericArguments().First();
                var repoType = typeof(LinqRepository<,,>).MakeGenericType(typeof(TDbContext), entityType, idType);
                var interfaceType = typeof(ILinqRepository<,>).MakeGenericType(entityType, idType);

                services.AddScoped(interfaceType, repoType);
            }

            return services;
        }
    }



}
