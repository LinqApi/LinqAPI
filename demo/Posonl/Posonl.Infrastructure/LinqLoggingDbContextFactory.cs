using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using LinqApi.Epoch;
using LinqApi.Logging;

namespace Posonl.Infrastructure
{
    public class PosOnlLogDbContextFactory : IDesignTimeDbContextFactory<LinqLoggingDbContext>
    {
        public LinqLoggingDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<LinqLoggingDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("logdb"));

            // Design-time context için default epoch provider kullanıyoruz.
            return new LinqLoggingDbContext(optionsBuilder.Options, new DefaultLinqEpochProvider());
        }
    }
}