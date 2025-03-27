using LinqApi.Model;
using LinqApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace LinqApi.Controller
{
    /// <summary>
    /// Controller that supports Create and Update operations via view models.
    /// Inherits from <see cref="LinqVmReadonlyController{TEntity, TCreateVm, TId}"/> where the create view model is used for schema.
    /// </summary>
    /// <typeparam name="TEntity">The entity type, which must derive from BaseEntity&lt;TId&gt;.</typeparam>
    /// <typeparam name="TCreateVm">The view model type for create operations.</typeparam>
    /// <typeparam name="TUpdateVm">The view model type for update operations.</typeparam>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    [Route("[controller]")]
    public abstract class LinqVmController<TEntity, TCreateVm, TUpdateVm, TId> : LinqVmReadonlyController<TEntity, TCreateVm, TId>
         where TEntity : BaseEntity<TId>
    {
        protected readonly ILinqRepository<TEntity, TId> _repo;

        protected LinqVmController(ILinqRepository<TEntity, TId> repo) : base(repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Creates a new entity from the provided create view model.
        /// </summary>
        /// <param name="vm">The create view model.</param>
        /// <param name="cancellation">A cancellation token.</param>
        /// <returns>The created entity.</returns>
        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] TCreateVm vm, CancellationToken cancellation)
        {
            var entity = MapToEntityFromCreate(vm);
            var created = await _repo.InsertAsync(entity, cancellation);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing entity using the provided update view model.
        /// </summary>
        /// <param name="id">The identifier of the entity to update.</param>
        /// <param name="vm">The update view model.</param>
        /// <param name="cancellation">A cancellation token.</param>
        /// <returns>The updated entity.</returns>
        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update([FromRoute] TId id, [FromBody] TUpdateVm vm, CancellationToken cancellation)
        {
            if (id == null)
                return BadRequest("Id is required.");

            try
            {
                var entity = MapToEntityFromUpdate(vm);
                var updated = await _repo.UpdateAsync(entity, cancellation);
                if (updated == null)
                    return NotFound($"Entity with id {id} was not found.");
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during update.", error = ex.Message });
            }
        }

        /// <summary>
        /// Maps the create view model to the entity.
        /// </summary>
        /// <param name="vm">The create view model.</param>
        /// <returns>The mapped entity.</returns>
        protected abstract TEntity MapToEntityFromCreate(TCreateVm vm);

        /// <summary>
        /// Maps the update view model to the entity.
        /// </summary>
        /// <param name="vm">The update view model.</param>
        /// <returns>The mapped entity.</returns>
        protected abstract TEntity MapToEntityFromUpdate(TUpdateVm vm);
    }
}
