using Microsoft.AspNetCore.Mvc;

namespace LinqApi.Wiki.Controllers
{
    public abstract class BaseController : Controller
    {
        protected void SetSeoDefaults(string title = null, string metaDescription = null, string canonicalUrl = null)
        {
            ViewData["Title"] = title ?? "LinqApi - Open Source Toolkit";
            ViewData["MetaDescription"] = metaDescription ?? "LinqApi provides a modern, lightweight set of tools for building dynamic APIs, extensible LINQ querying, and more.";
            ViewData["CanonicalUrl"] = canonicalUrl ?? "https://linqapi.com";
        }
    }
}
