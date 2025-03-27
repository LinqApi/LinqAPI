using System.Collections.Concurrent;
using System.Reflection;
using LinqApi.Model;
using LinqApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace LinqApi.Controller
{
    /// <summary>
    /// Base controller for read-only endpoints that expose a view model schema.
    /// Inherits from <see cref="LinqReadonlyController{TEntity, TId}"/>.
    /// Provides an endpoint to retrieve the schema of the specified view model type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type, which must derive from BaseEntity&lt;TId&gt;.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model to generate schema from.</typeparam>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    [ApiController]
    [Route("[controller]")]
    public abstract class LinqVmReadonlyController<TEntity, TViewModel, TId> : LinqReadonlyController<TEntity, TId>
         where TEntity : BaseEntity<TId>
    {
        private static readonly ConcurrentDictionary<string, object> _vmSchemaCache = new();

        protected LinqVmReadonlyController(ILinqRepository<TEntity, TId> repo) : base(repo)
        {
        }

        /// <summary>
        /// Returns a schema description of the view model type.
        /// </summary>
        /// <returns>An object representing the view model properties.</returns>
        [HttpGet("vm-schema")]
        public virtual IActionResult GetVmSchema()
        {
            var vmType = typeof(TViewModel);
            var cacheKey = $"VmSchema_{vmType.FullName}";
            if (_vmSchemaCache.TryGetValue(cacheKey, out var cached))
                return Ok(cached);

            var instance = Activator.CreateInstance(vmType);
            var props = vmType.GetProperties()
                .Select(p => new
                {
                    Name = p.Name,
                    Type = GetReadableTypeName(p.PropertyType),
                    Default = p.GetValue(instance),
                    IsRequired = p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("Required"))
                })
                .Cast<object>()
                .ToList();
            _vmSchemaCache.TryAdd(cacheKey, props);
            return Ok(props);
        }
    }
}
