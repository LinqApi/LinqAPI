using Microsoft.AspNetCore.Mvc;

namespace Posonl.Web.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Example() => View();
        public IActionResult Country() => View();
        public IActionResult CountryGroup() => View();
        public IActionResult PosCompany() => View();
        public IActionResult PosService() => View();
        public IActionResult PosServiceCategory() => View();
        public IActionResult PosCommissionRate() => View();
        public IActionResult RatingCategory() => View();
        public IActionResult LinqSqlLogs() => View();
        public IActionResult LinqHttpCallLogs() => View();
        public IActionResult LinqEventLogs() => View();
        public IActionResult Healthcheck() => View();
    }
}
