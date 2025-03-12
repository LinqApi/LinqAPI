using LinqApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;
using TableInfo = LinqApi.Model.TableInfo;

namespace LinqApi.Dynamic.Controller
{
    public class LinqMvcController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly Dictionary<string, LinqMsApiConfiguration> _configurations;

        public LinqMvcController(IServiceProvider serviceProvider)
        {
            // LinqMsApiRegistry.Configurations statik alandan veya DI üzerinden alınabilir.
            _configurations = LinqMsApiRegistry.Configurations;
        }

        public IActionResult Index()
        {
            // Tüm konfigürasyonlardaki entity bilgilerini toplayalım.
            var model = _configurations.SelectMany(cfg =>
                cfg.Value.DynamicEntities.Keys.Select(key =>
                {
                    // key: "schema.table" ve ayrıca cfg.Value.AreaName var.
                    var parts = key.Split('.');
                    string schema;
                    string table;
                    string displayName = string.Empty;
                    if (parts.Length == 2)
                    {
                        schema = parts[0].ToLower();
                        table = parts[1];
                        displayName = schema == "dbo" || string.IsNullOrWhiteSpace(schema)
                           ? table
                        : $"{schema}.{table}";
                    }
                    else
                    {
                        schema = "dbo";
                        table = parts[0];
                        displayName = $"dbo.{table}";
                    }

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
