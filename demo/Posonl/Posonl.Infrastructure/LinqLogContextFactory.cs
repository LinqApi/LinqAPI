using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using LinqApi.Epoch;
using LinqApi.Logging;

namespace Posonl.Infrastructure
{
    public class LinqLogContextFactory : IDesignTimeDbContextFactory<LinqLogContext>
    {
        public LinqLogContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<LinqLoggingDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("logdb"));
            return new LinqLogContext(optionsBuilder.Options, new DefaultLinqEpochProvider());
        }
    }
}