using LinqApi.Model;

namespace LinqApi.Demo.Data
{
    public class MyEntity : BaseEntity<long>
    {
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
