using Microsoft.EntityFrameworkCore;
using LinqApi.Epoch;
using LinqApi.Localization;

namespace Posonl.Infrastructure
{
    /// <summary>
    /// A specialized DbContext for localization that inherits from LinqLocalizationDbContext.
    /// </summary>
    public class LinqLocalizationContext : LinqLocalizationDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqLocalizationContext"/> class.
        /// </summary>
        /// <param name="options">The options for this DbContext.</param>
        /// <param name="epochProvider">The epoch provider instance.</param>
        public LinqLocalizationContext(DbContextOptions<LinqLocalizationDbContext> options, IEpochProvider epochProvider)
            : base(options, "posonl")
        {
        }
    }
}