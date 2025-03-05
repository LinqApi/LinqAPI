using AutoMapper;
using LinqApi.Repository;
using LinqApi.Service;
using Microsoft.Extensions.DependencyInjection;

namespace LinqApi.Helpers
{
    public static class LinqApiServiceCollectionExtensions
    {
        /// <summary>
        /// ILinqRepository ve ILinqService arayüzleri için açık generik kayıt ekler.
        /// </summary>
        public static IServiceCollection AddLinqApi(this IServiceCollection services)
        {
            // Repository ve Service kayıtları
            services.AddScoped(typeof(ILinqRepository<,>), typeof(LinqRepository<,>));
            services.AddScoped(typeof(ILinqService<,,>), typeof(LinqService<,,>));

            // AutoMapper kaydı: otomatik mapping profilini ekle.
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoLinqMappingProfile>();
            });
            services.AddSingleton<IMapper>(mapperConfig.CreateMapper());

            return services;
        }
    }

}
