using LinqApi.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace LinqApi.Controller
{
    public class LinqMSController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ConcurrentDictionary<string, Type> _dynamicEntities;

        public LinqMSController(ConcurrentDictionary<string, Type> dynamicEntities)
        {
            _dynamicEntities = dynamicEntities;
        }

        public IActionResult Index()
        {
            // Örneğin, key "dbo.Orders" veya "sales.Customers" gibi
            var model = _dynamicEntities.Keys
                .Select(key => {
                    var parts = key.Split('.');
                    var schema = parts[0].ToLower();
                    var table = parts[1];
                    var displayName = (schema == "dbo" || string.IsNullOrWhiteSpace(schema))
                        ? table
                        : $"{schema}.{table}";
                    return new TableInfo
                    {
                        Key = key,
                        DisplayName = displayName
                    };
                })
                .OrderBy(t => t.DisplayName)
                .ToList();

            return View("LinqMS", model);
        }

    }



}
