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
            // Use the request information to build the base URL.
            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";

            // Define the URLs for your sitemap.
            var urls = new[]
            {
                $"{baseUrl}/",                          // Home
                $"{baseUrl}/contact",                   // Contact
                $"{baseUrl}/documentation/docs",        // Docs
                $"{baseUrl}/documentation/getstarted",  // GetStarted
                $"{baseUrl}/documentation/nuget",         // NuGet
                $"{baseUrl}/documentation/javascript"     // JavaScript
                // Additional URLs (e.g., search, blog) can be added here.
            };

            // Define the XML namespace.
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            // Build the sitemap XML elements.
            var sitemapElement = new XElement(ns + "urlset",
                from url in urls
                select new XElement(ns + "url",
                    new XElement(ns + "loc", url),
                    new XElement(ns + "changefreq", "weekly"),
                    new XElement(ns + "priority", url.EndsWith("/") ? "1.0" : "0.8")
                )
            );

            // Create an XDocument that includes the XML declaration.
            var xmlDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                sitemapElement
            );

            // Convert the XML document to a string without extra formatting.
            string xmlString = xmlDocument.ToString(SaveOptions.DisableFormatting);

            // Return the XML string with the correct content type and encoding.
            return Content(xmlString, "application/xml", Encoding.UTF8);
        }
    }
}
