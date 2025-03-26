namespace LinqApi.Model
{
    public abstract class BaseEntity<TId>
    {
        public TId Id { get; set; }
    }


}
