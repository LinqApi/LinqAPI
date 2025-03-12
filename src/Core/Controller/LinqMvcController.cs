using LinqApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace LinqApi.Controller
{
    public class LinqMvcDynamicController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly Dictionary<string, LinqMsApiConfiguration> _configurations;

        public LinqMvcDynamicController(IServiceProvider serviceProvider)
        {
            // LinqMsApiRegistry.Configurations statik alandan veya DI üzerinden alınabilir.
            _configurations = LinqMsApiRegistry.Configurations;
        }

        public IActionResult Index()
        {
            // Tüm konfigürasyonlardaki entity bilgilerini toplayalım.
            var model = _configurations.SelectMany(cfg =>
                cfg.Value.DynamicEntities.Keys.Select(key => {
                    // key: "schema.table" ve ayrıca cfg.Value.AreaName var.
                    var parts = key.Split('.');
                    string schema = parts[0].ToLower();
                    string table = parts[1];
                    string displayName = (schema == "dbo" || string.IsNullOrWhiteSpace(schema))
                        ? table
                        : $"{schema}.{table}";
                    return new TableInfo
                    {
                        Key = key,
                        DisplayName = displayName,
                        SchemaName = schema,
                        Area = cfg.Value.AreaName
                    };
                })
            ).OrderBy(t => t.DisplayName)
             .ToList();

            return View("LinqMvc", model);
        }
    }



}
