namespace LinqApi.Core
{
    public class ExtendLinqApiAttribute<TEntity, TId> : Attribute where TEntity : BaseEntity<TId>
    {
    }
}
