using LinqApi.Correlation;
using LinqApi.Logging;
using LinqApi.Logging.LinqApi.Core;
using LinqApi.Logging.Log;
using LinqApi.Logging.Module;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinqApi.Logging
{
    public class LinqLoggingModule : IDbContextModule
    {
        private readonly string _schema = "log";
        private readonly Action<IServiceProvider, DbContextOptionsBuilder> _configureDb;

        public LinqLoggingModule(Action<IServiceProvider, DbContextOptionsBuilder> configureDb)
        {
            _configureDb = configureDb ?? throw new ArgumentNullException(nameof(configureDb));
        }

        public void RegisterServices(IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<LinqLoggingDbContext>(_configureDb);
            services.AddScoped<ILogRule, TimestampLogRule>();
            services.AddScoped<ICorrelationIdGenerator, DefaultCorrelationIdGenerator>();
            services.AddScoped<LinqSqlLoggingInterceptor>();
            services.AddScoped<ILogRule, CreatedByLogRule>();
            services.AddScoped<LinqLoggingBehavior>();
            //services.AddScoped<LinqHttpLoggingMiddleware>();

            services.AddScoped<LinqLoggingOptions>();



            services.AddScoped<ILinqPayloadMasker, DefaultPayloadMasker>();
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
