using LinqApi.Localization.LinqApi.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Posonl.Domain;
using Posonl.Infrastructure;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Posonl.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PosOnlDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, PosOnlDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IActionResult Index(string lang)
        {
            if (string.IsNullOrEmpty(lang))
            {
                var rqf = HttpContext.Features.Get<IRequestCultureFeature>();
                lang = rqf?.RequestCulture.Culture.TwoLetterISOLanguageName ?? "tr";
            }

            ViewBag.SelectedLang = lang;

            string fullCulture = lang switch
            {
                "en" => "en-US",
                "de" => "de-DE",
                _ => "tr-TR"
            };

            CultureInfo.CurrentCulture = new CultureInfo(fullCulture);
            CultureInfo.CurrentUICulture = new CultureInfo(fullCulture);

            ViewBag.Countries = _dbContext.Countries.OrderBy(c => c.Name).ToList();
            ViewBag.PosServiceCategories = _dbContext.PosServiceCategories
                .Include(p => p.PosServices)
                .ToList();

            return View();
        }


        [HttpPost]
        public IActionResult SetCulture(string lang, string returnUrl)
        {
            lang = string.IsNullOrEmpty(lang) ? "tr" : lang;

            var culture = lang switch
            {
                "en" => "en-US",
                "de" => "de-DE",
                _ => "tr-TR"
            };

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            // returnUrl'deki mevcut dil prefixlerini temizle
            if (!string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Regex.Replace(returnUrl, @"^\/(tr|en|de)(\/)?", "/");
            }

            return LocalRedirect($"/{lang}{returnUrl}");
        }

    }

    [ApiController]
    [Route("[controller]")]
    public class InController : ControllerBase
    {
        private readonly DbContext _db;
        public InController(DbContext db) => _db = db;

        [HttpGet("{type}/{slug}")]
        public IActionResult Detail(string type, string slug)
        {
            BaseViewEntity entity = type.ToLowerInvariant() switch
            {
                "poscompany" => _db.Set<PosCompany>().FirstOrDefault(x => x.Slug == slug),
                "posservice" => _db.Set<PosService>().FirstOrDefault(x => x.Slug == slug),
                "news" => _db.Set<News>().FirstOrDefault(x => x.Slug == slug),
                _ => null
            };

            if (entity == null) return NotFound();
            return Ok(entity);
        }
    }



}