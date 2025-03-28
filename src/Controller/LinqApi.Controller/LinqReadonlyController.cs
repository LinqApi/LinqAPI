using LinqApi.Core;
using LinqApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace LinqApi.Controller
{
    /// <summary>
    /// Base controller for read-only LINQ-powered endpoints.
    /// Provides dynamic filtering, paging, ordering, and metadata discovery.
    /// </summary>
    /// <typeparam name="TEntity">The entity type, which must derive from BaseEntity&lt;TId&gt;.</typeparam>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    [ApiController]
    [Route("api/[controller]")]
    public class LinqReadonlyController<TEntity, TId> : ControllerBase
        where TEntity : BaseEntity<TId>
    {
        protected readonly ILinqRepository<TEntity, TId> _repo;
        protected LinqReadonlyController(ILinqRepository<TEntity, TId> repo)
        {
            _repo = repo;
        }

        [HttpGet("properties")]

        public virtual IActionResult GetAllProperties()
        {

            var props = ViewModelSchemaHelper.GetSchema(typeof(TEntity));
            return Ok(props);
        }

        /// <summary>
        /// Gets an entity by its identifier.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="cancellation">A cancellation token.</param>
        /// <returns>The entity if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(TId id, CancellationToken cancellation)
        {
            var entity = await _repo.GetByIdAsync(id, cancellation);
            return entity == null ? NotFound() : Ok(entity);
        }

        /// <summary>
        /// Filters the entity collection with paging.
        /// </summary>
        /// <param name="model">The filter model.</param>
        /// <param name="cancellation">A cancellation token.</param>
        /// <returns>A paged result of entities.</returns>
        [HttpPost("filterpaged")]
        public virtual async Task<IActionResult> GetByFilterPaged([FromBody] LinqFilterModel model, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(model.Filter))
                return BadRequest("Filter is required.");
            var result = await _repo.GetFilterPagedAsync(model, cancellation);
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
