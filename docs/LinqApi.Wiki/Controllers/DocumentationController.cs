using Microsoft.AspNetCore.Mvc;

namespace LinqApi.Wiki.Controllers
{
    public class DocumentationController : BaseController
    {
        // NuGet page
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public IActionResult NuGet()
        {
            SetSeoDefaults(
                title: "NuGet Packages - LinqApi",
                metaDescription: "Explore our robust and extensible NuGet packages built for high performance and scalability.",
                canonicalUrl: "https://linqapi.com/nuget"
            );
            return View();
        }

        // JavaScript Libraries page
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public IActionResult JavaScript()
        {
            SetSeoDefaults(
                title: "JavaScript Libraries - LinqApi",
                metaDescription: "Discover our lightweight, performant JavaScript libraries that empower your projects with dynamic functionality.",
                canonicalUrl: "https://linqapi.com/javascript"
            );
            return View();
        }

        // General Documentation page
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public IActionResult Docs()
        {
            SetSeoDefaults(
                title: "Documentation - LinqApi",
                metaDescription: "Browse comprehensive guides and API documentation to get started with LinqApi quickly.",
                canonicalUrl: "https://linqapi.com/docs"
            );
            return View();
        }

        // Get Started page
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public IActionResult GetStarted()
        {
            SetSeoDefaults(
                title: "Get Started - LinqApi",
                metaDescription: "Follow our step-by-step guides to integrate LinqApi into your projects and elevate your development workflow.",
                canonicalUrl: "https://linqapi.com/getstarted"
            );
            return View();
        }
    }
}
