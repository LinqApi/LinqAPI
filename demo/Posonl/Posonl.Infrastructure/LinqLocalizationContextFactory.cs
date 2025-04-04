using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using LinqApi.Localization;
using LinqApi.Epoch;

namespace Posonl.Infrastructure
{
    /// <summary>
    /// Design-time factory for creating instances of LinqLocalizationContext.
    /// </summary>
    public class LinqLocalizationContextFactory : IDesignTimeDbContextFactory<LinqLocalizationContext>
    {
        /// <summary>
        /// Creates a new instance of <see cref="LinqLocalizationContext"/> using configuration from appsettings.json.
        /// </summary>
        /// <param name="args">Arguments passed to the application.</param>
        /// <returns>An instance of LinqLocalizationContext.</returns>
        public LinqLocalizationContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: false)
                 .Build();

            var optionsBuilder = new DbContextOptionsBuilder<LinqLocalizationDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("localizationdb"));
            // Use the appropriate epoch provider.
            return new LinqLocalizationContext(optionsBuilder.Options, new DefaultLinqEpochProvider());
        }
    }
}