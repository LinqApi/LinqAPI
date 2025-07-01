using LinqApi.Logging.Module;
using LinqApi.Localization.LinqApi.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


namespace LinqApi.Localization
{
    public class LinqLocalizationModule : IDbContextModule
    {
        private readonly string _schema = "loc";
        private readonly Action<IServiceProvider, DbContextOptionsBuilder> _configureDb;

        public LinqLocalizationModule(

            Action<IServiceProvider, DbContextOptionsBuilder> configureDb)
        {
            _configureDb = configureDb ?? throw new ArgumentNullException(nameof(configureDb));
        }

        public void RegisterServices(IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<LinqLocalizationDbContext>(_configureDb);

            services.AddScoped<ILinqLocalizationDbContextAdapter>(sp =>
                sp.GetRequiredService<LinqLocalizationDbContext>());
        }

        public IEnumerable<Type> GetEntityTypes() => new[]
        {
        typeof(LinqLocalizationEntity)
    };

        public void ApplyModel(ModelBuilder builder)
        {
            builder.ApplyLocalizationModel(_schema);
        }
    }



}