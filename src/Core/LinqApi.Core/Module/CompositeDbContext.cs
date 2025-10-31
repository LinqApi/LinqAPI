using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinqApi.Logging.Module
{

    /// <summary>
    /// Defines a module that can register services and apply EF Core model mappings.
    /// </summary>
    public interface IDbContextModule
    {
        /// <summary>
        /// Register module-specific services (behaviors, providers, etc.)
        /// </summary>
        void RegisterServices(IServiceCollection services, IConfiguration config);

        /// <summary>
        /// Apply EF Core model configuration (OnModelCreating).
        /// </summary>
        void ApplyModel(ModelBuilder builder);


    }
    public class ModuleRegistry
    {
        public List<IDbContextModule> Modules { get; } = new();

        public void AddModule(IDbContextModule module)
        {
            Modules.Add(module);
        }
    }

    public class CompositeDbContext : DbContext
    {
        private readonly IServiceProvider _sp;

        public CompositeDbContext(DbContextOptions options, IServiceProvider sp)
            : base(options)
        {
            _sp = sp;
        }


        protected override void OnModelCreating(ModelBuilder mb)
        {

            var modules = _sp.GetService<IEnumerable<IDbContextModule>>() ?? Enumerable.Empty<IDbContextModule>();

        }
    }
}
