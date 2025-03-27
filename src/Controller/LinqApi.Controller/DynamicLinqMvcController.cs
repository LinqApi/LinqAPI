using LinqApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace LinqApi.Dynamic.Controller
{
    public abstract class LinqMvcController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly Dictionary<string, LinqMsApiConfiguration> _configurations;

        public LinqMvcController(IServiceProvider serviceProvider)
        {
            // LinqMsApiRegistry.Configurations statik alandan veya DI üzerinden alınabilir.
            _configurations = LinqMsApiRegistry.Configurations;
        }

        public virtual IActionResult Index()
        {
            // Tüm konfigürasyonlardaki entity bilgilerini toplayalım.
            var model = _configurations.SelectMany(cfg =>
                cfg.Value.DynamicEntities.Keys.Select(key => {
                    // key: "schema.table" veya sadece "table" gelebilir.
                    var parts = key.Split('.');

                    string schema, table;
                    if (parts.Length == 1)
                    {
                        // Eğer sadece tablo adı geldiyse, şema olarak "dbo" varsayılıyor.
                        schema = "dbo";
                        table = parts[0];
                    }
                    else
                    {
                        schema = parts[0].ToLower();
                        table = parts[1];
                    }

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