namespace LinqApi.Logging
{
    /// <summary>
    /// Provides a base class for all entities with a strongly typed primary key.
    /// </summary>
    /// <typeparam name="TId">The type of the primary key (e.g., <see cref="Guid"/>, <see cref="long"/>, <see cref="int"/>).</typeparam>
    public abstract class BaseEntity<TId>
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public virtual TId Id { get; set; }
    }
}