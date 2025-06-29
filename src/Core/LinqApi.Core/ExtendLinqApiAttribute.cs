namespace LinqApi.Logging
{
    public class ExtendLinqApiAttribute<TEntity, TId> : Attribute where TEntity : BaseEntity<TId>
    {
    }
}
