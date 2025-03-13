using LinqApi.Model;

namespace LinqApi.Demo.Data
{
    public class MyTestEntity : BaseEntity<int>
    {
        public bool IsTest { get { return true; } }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
