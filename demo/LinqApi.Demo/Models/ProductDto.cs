using LinqApi.Model;

namespace LinqApi.Demo.Models
{
    public class ProductDto : BaseDto<int>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
