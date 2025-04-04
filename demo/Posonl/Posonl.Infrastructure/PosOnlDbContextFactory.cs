using LinqApi.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace Posonl.Infrastructure
{
    public class PosOnlDbContextFactory : IDesignTimeDbContextFactory<PosOnlDbContext>
    {
        public PosOnlDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<LinqLocalizationDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("PosOnlDb"));

            return new PosOnlDbContext(optionsBuilder.Options,"posonl");
        }
    }
}
