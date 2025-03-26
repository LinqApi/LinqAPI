using LinqApi.Model;

namespace LinqApi.Controller
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using System;
    using LinqApi.Repository;
    using System.Collections.Concurrent;
    using System.Linq;

    /// <summary>
    /// Base class for read-only LINQ-powered endpoints.
    /// Provides dynamic filter, paging, ordering, and metadata discovery support.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class LinqReadonlyController<TEntity, TId> : ControllerBase
        where TEntity : BaseEntity<TId>
    {
        protected readonly ILinqRepository<TEntity, TId> _repo;
        private static readonly ConcurrentDictionary<string, List<object>> _propertyCache = new();

        protected LinqReadonlyController(ILinqRepository<TEntity, TId> repo)
        {
            _repo = repo;
        }

        [HttpGet("properties")]
        public virtual IActionResult GetAllProperties()
        {
            var type = typeof(TEntity);
            var cacheKey = $"Properties_{type.FullName}";
            if (_propertyCache.TryGetValue(cacheKey, out var cached)) return Ok(cached);

            var props = type.GetProperties()
                .Select(p => new { Name = p.Name, Type = GetReadableTypeName(p.PropertyType) })
                .Cast<object>()
                .ToList();

            _propertyCache.TryAdd(cacheKey, props);
            return Ok(props);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(TId id, CancellationToken cancellation)
        {
            var entity = await _repo.GetByIdAsync(id,cancellation);
            return entity == null ? NotFound() : Ok(entity);
        }

        [HttpPost("filterpaged")]
        public virtual async Task<IActionResult> GetByFilterPaged([FromBody] LinqFilterModel model, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(model.Filter)) return BadRequest("Filter is required.");
            var result = await _repo.GetFilterPagedAsync(model,cancellation);
            return Ok(result);
        }

        protected string GetReadableTypeName(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return $"{Nullable.GetUnderlyingType(type)?.Name}?";
            if (type.IsGenericType)
                return $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(GetReadableTypeName))}>";
            return type.Name;
        }
    }
}