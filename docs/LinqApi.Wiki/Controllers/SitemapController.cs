using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;

namespace LinqApi.Wiki.Controllers
{
    [Route("sitemap.xml")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public class SitemapController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";

            var urls = new[]
            {
                $"{baseUrl}/",                          // Home
                $"{baseUrl}/contact",                  // Contact
                $"{baseUrl}/documentation/docs",       // Docs
                $"{baseUrl}/documentation/getstarted", // GetStarted
                $"{baseUrl}/documentation/nuget",      // NuGet
                $"{baseUrl}/documentation/javascript"  // JavaScript
                // Eklemek istersen search gibi şeyler de burada olabilir
            };

            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var sitemap = new XElement(ns + "urlset",
                from url in urls
                select new XElement(ns + "url",
                    new XElement(ns + "loc", url),
                    new XElement(ns + "changefreq", "weekly"),
                    new XElement(ns + "priority", url.EndsWith("/") ? "1.0" : "0.8")
                )
            );

            var xml = new XDocument(sitemap);
            var xmlString = new UTF8Encoding(false).GetBytes(xml.ToString());

            return File(xmlString, "application/xml");
        }
    }
}
