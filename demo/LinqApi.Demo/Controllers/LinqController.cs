using LinqApi.Controller;
using LinqApi.Demo.Data;
using LinqApi.Repository;

namespace LinqApi.Demo.Controllers
{
    public class MyEntityController : LinqController<MyEntity, long>
    {
        public MyEntityController(ILinqRepository<MyEntity, long> repo) : base(repo)
        {
        }

        
    }

    public class MyTestEntityController : LinqController<MyTestEntity, int>
    {
        public MyTestEntityController(ILinqRepository<MyTestEntity, int> repo) : base(repo)
        {
        }
    }
}
