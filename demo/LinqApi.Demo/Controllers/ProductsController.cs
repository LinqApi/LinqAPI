using LinqApi.Controller;
using LinqApi.Demo.Models;
using LinqApi.Service;

namespace LinqApi.Demo.Controllers
{
    public class ProductsController : LinqController<ProductEntity, ProductDto, int>
    {
        public ProductsController(ILinqService<ProductEntity, ProductDto, int> service) : base(service)
        {
        }
    }
}
