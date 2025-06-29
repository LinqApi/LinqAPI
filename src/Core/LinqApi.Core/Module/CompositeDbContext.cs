using Microsoft.EntityFrameworkCore;
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
        void RegisterServices(IServiceCollection services);

        /// <summary>
        /// Apply EF Core model configuration (OnModelCreating).
        /// </summary>
        void ApplyModel(ModelBuilder builder);

        /// <summary>
        /// Module, hangi entity tiplerini expose ettiğini bildirir.
        /// Bu sayede DbContext, migration ve discovery aşamasında tüm tipleri
        /// modelBuilder.Entity(...) ile ekleyebilir.
        /// </summary>
        IEnumerable<Type> GetEntityTypes();
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
        private readonly IEnumerable<IDbContextModule> _modules;

        public CompositeDbContext(
          DbContextOptions<CompositeDbContext> opts,
          IEnumerable<IDbContextModule> modules)
          : base(opts)
        {
            _modules = modules;
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // 1) Discover & register tüm entity tipleri
            var allTypes = _modules.SelectMany(m => m.GetEntityTypes()).Distinct();
            foreach (var t in allTypes)
                mb.Model.AddEntityType(t);

            // 2) Uygula modül bazlı mapping’leri
            foreach (var module in _modules)
                module.ApplyModel(mb);
        }
    }
}
