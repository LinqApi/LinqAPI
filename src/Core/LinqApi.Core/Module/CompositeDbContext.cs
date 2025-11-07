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
        void RegisterServices (IServiceCollection services, IConfiguration config);

        /// <summary>
        /// Apply EF Core model configuration (OnModelCreating).
        /// </summary>
        void ApplyModel (ModelBuilder builder);
    }
    public class ModuleRegistry
    {
        public List<IDbContextModule> Modules { get; } = new();

        public void AddModule (IDbContextModule module)
        {
            Modules.Add(module);
        }
    }

    public interface IAuditHook
    {
        public Task OnSavedAsync (DbContext ctx, int rows, CancellationToken ct);
        public Task OnSavingAsync (DbContext ctx, CancellationToken ct);
    }
    public sealed class NoAuditHook : IAuditHook
    {
        public Task OnSavingAsync (DbContext ctx, CancellationToken ct) => Task.CompletedTask;
        public Task OnSavedAsync (DbContext ctx, int rows, CancellationToken ct) => Task.CompletedTask;
    }

    public class CompositeDbContext : DbContext
    {
        private readonly IServiceProvider _sp;
        private readonly IAuditHook _audit;

        public CompositeDbContext (DbContextOptions options, IServiceProvider sp)
            : base(options)
        {
            _sp = sp;
            _audit = sp.GetService<IAuditHook>() ?? new NoAuditHook();
        }

        protected override void OnModelCreating (ModelBuilder mb)
        {
            var modules = _sp.GetService<IEnumerable<IDbContextModule>>() ?? Enumerable.Empty<IDbContextModule>();
        }

        public override async Task<int> SaveChangesAsync (CancellationToken ct = default)
        {
            await _audit.OnSavingAsync(this, ct);
            var rows = await base.SaveChangesAsync(ct);
            await _audit.OnSavedAsync(this, rows, ct);
            return rows;
        }
        public override int SaveChanges ()
        {
            _audit.OnSavingAsync(this, default).GetAwaiter().GetResult();
            var rows = base.SaveChanges();
            _audit.OnSavedAsync(this, rows, default).GetAwaiter().GetResult();
            return rows;
        }
    }
}
