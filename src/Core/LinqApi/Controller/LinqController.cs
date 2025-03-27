using LinqApi.Model;
using System.Linq.Dynamic.Core;

namespace LinqApi.Controller
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System;
    using LinqApi.Repository;
    using System.Reflection;

    [ApiController]
    [Route("api/[controller]")]
    public class LinqController<TEntity, TId>(ILinqRepository<TEntity, TId> repo) : LinqReadonlyController<TEntity, TId>(repo)
     where TEntity : BaseEntity<TId>
    {
        protected internal readonly ILinqRepository<TEntity, TId> _repo;

        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] TEntity entity, CancellationToken cancellationToken)
        {
            var createdDto = await _repo.InsertAsync(entity, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdDto.Id }, createdDto);
        }

        [HttpPut]
        public virtual async Task<IActionResult> Update([FromBody] TEntity entity, CancellationToken cancellationToken)
        {
            var updatedDto = await _repo.UpdateAsync(entity, cancellationToken);
            return Ok(updatedDto);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(TId id, CancellationToken cancellationToken)
        {
            await _repo.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }

    /// <summary>
    /// Provides helper methods to generate a property schema for entity types.
    /// </summary>
    public static class EntitySchemaHelper
    {
        /// <summary>
        /// Generates a list of property descriptors for the specified entity type.
        /// Each descriptor contains the property name and a readable type name.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <returns>A list of anonymous objects with Name and Type properties.</returns>
        public static List<object> GetPropertiesSchema(Type entityType)
        {
            return entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Name = p.Name,
                    Type = GetReadableTypeName(p.PropertyType)
                })
                .Cast<object>()
                .ToList();
        }

        /// <summary>
        /// Returns a human-friendly name for a given type.
        /// For example, nullable types will have a trailing "?".
        /// </summary>
        /// <param name="type">The type to process.</param>
        /// <returns>A string representing the type name.</returns>
        private static string GetReadableTypeName(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return $"{Nullable.GetUnderlyingType(type)?.Name}?";
            }

            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments().Select(GetReadableTypeName);
                return $"{type.Name.Split('`')[0]}<{string.Join(", ", genericArguments)}>";
            }

            return type.Name;
        }
    }
}
