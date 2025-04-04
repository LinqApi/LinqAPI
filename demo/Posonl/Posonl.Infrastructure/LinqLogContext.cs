using Microsoft.EntityFrameworkCore;
using LinqApi.Epoch;
using LinqApi.Logging;

namespace Posonl.Infrastructure
{
    public class LinqLogContext : LinqLoggingDbContext
    {
        public LinqLogContext(DbContextOptions<LinqLoggingDbContext> options, IEpochProvider epochProvider)
            : base(options, epochProvider)
        {
        }
    }
}