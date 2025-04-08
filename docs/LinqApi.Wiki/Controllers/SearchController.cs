using Microsoft.AspNetCore.Mvc;

namespace LinqApi.Wiki.Controllers
{
    public class SearchController : BaseController
    {
        // Search page – caches for a shorter duration since query results might change
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public IActionResult Index(string query)
        {
            SetSeoDefaults(
                title: "Search - LinqApi",
                metaDescription: "Search through LinqApi documentation, NuGet packages, and JavaScript libraries.",
                canonicalUrl: "https://linqapi.com/search"
            );

            // For now, simply pass the query to the view.
            // Later, you can implement actual search logic to retrieve results.
            ViewData["Query"] = query;
            return View();
        }
    }
}
