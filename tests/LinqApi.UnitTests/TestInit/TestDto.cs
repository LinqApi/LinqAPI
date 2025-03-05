using LinqApi.Model;

namespace LinqApi.UnitTests.TestInit
{
    public class TestDto : BaseDto<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
