using LinqApi.Logging;
using LinqApi.Logging.Log;
using LinqApi.Logging.Module;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LinqApi.Logging
{
    public class LinqLoggingModule : IDbContextModule
    {
        private readonly string _schema;
        private readonly Action<IServiceProvider, DbContextOptionsBuilder> _configureDb;

        public LinqLoggingModule(string schema, Action<IServiceProvider, DbContextOptionsBuilder> configureDb)
        {
            _schema = schema;
            _configureDb = configureDb ?? throw new ArgumentNullException(nameof(configureDb));
        }

        public void RegisterServices(IServiceCollection services)
        {
            services.AddDbContext<LinqLoggingDbContext>(_configureDb);
            services.AddScoped<ILinqLogger, LinqDbContextCallLogger>();
            services.AddScoped<ILinqLoggingDbContextAdapter>(sp =>
                sp.GetRequiredService<LinqLoggingDbContext>());
        }

        public IEnumerable<Type> GetEntityTypes() => new[]
        {
        typeof(LinqLogEntity),
        typeof(LinqEventLog),
        typeof(LinqHttpCallLog),
        typeof(LinqConsumeErrorLog),
        typeof(LinqPublishErrorLog),
        typeof(LinqSqlLog),
        typeof(LinqSqlErrorLog)
    };

        public void ApplyModel(ModelBuilder builder) => builder.ApplyLoggingModel(_schema);
    }




}
