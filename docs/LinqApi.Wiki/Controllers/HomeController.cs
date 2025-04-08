using LinqApi.Wiki.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LinqApi.Wiki.Controllers
{
    public class HomeController : BaseController
    {
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public IActionResult Index()
        {
            SetSeoDefaults(
                title: "Home - LinqApi",
                metaDescription: "Welcome to LinqApi – your modern, lightweight toolkit for building dynamic APIs and extensible LINQ querying.",
                canonicalUrl: "https://linqapi.com/"
            );
            return View();
        }

        // Contact page
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public IActionResult Contact()
        {
            SetSeoDefaults(
                title: "Contact - LinqApi",
                metaDescription: "Get in touch with the LinqApi team for inquiries, support, and collaboration opportunities.",
                canonicalUrl: "https://linqapi.com/contact"
            );
            return View();
        }
    }
}
