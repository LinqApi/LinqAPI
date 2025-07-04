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
        private readonly IServiceProvider _sp;

        public CompositeDbContext(DbContextOptions options, IServiceProvider sp)
            : base(options)
        {
            _sp = sp;
        }


        protected override void OnModelCreating(ModelBuilder mb)
        {

            var modules = _sp.GetService<IEnumerable<IDbContextModule>>() ?? Enumerable.Empty<IDbContextModule>();
            // 1) Discover & register tüm entity tipleri
            //var allTypes = modules.SelectMany(m => m.GetEntityTypes()).Distinct();
            //foreach (var t in allTypes)
            //    mb.Model.AddEntityType(t);

            //foreach (var module in modules)
            //    module.ApplyModel(mb);
        }
    }
}
