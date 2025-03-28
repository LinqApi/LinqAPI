using LinqApi.Dynamic;
using LinqApi.Dynamic.Assembly;
using LinqApi.Dynamic.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace LinqApi.Razor.Controllers
{
    /// <summary>
    /// Represents an abstract base controller for dynamically handling LINQ-based MVC operations.
    /// </summary>
    /// <remarks>
    /// This controller aggregates dynamic entity configurations from a central registry (or DI)
    /// and exposes an Index action to display available table information. For better separation
    /// of concerns, consider extracting the logic for mapping configurations to view models into
    /// a dedicated service if the complexity increases.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="LinqDynamicMvcController"/> class.
    /// </remarks>
    /// <param name="serviceProvider">
    /// The service provider used to resolve dependencies. Although not directly used here,
    /// it allows for future expansion or retrieval of configurations via DI.
    /// </param>
    public abstract class LinqDynamicMvcController(IServiceProvider serviceProvider) : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly Dictionary<string, LinqMsApiConfiguration> _configurations = LinqMsApiRegistry.Configurations;

        /// <summary>
        /// Gets the dynamic entity table information and returns the corresponding view.
        /// </summary>
        /// <returns>
        /// A view result named "LinqMvc" with a list of <see cref="TableInfo"/> items representing
        /// the available dynamic entities.
        /// </returns>
        /// <remarks>
        /// The method aggregates table information from all registered configurations. It supports keys
        /// formatted as "schema.table" or just "table", defaulting to the "dbo" schema when only a table name is provided.
        /// </remarks>
        public virtual IActionResult Index()
        {
            // Aggregate table information from all configurations.
            var model = _configurations.SelectMany(cfg =>
                cfg.Value.DynamicEntities.Keys.Select(key =>
                {
                    // The key may be in the format "schema.table" or only "table".
                    var parts = key.Split('.');
                    string schema, table;

                    if (parts.Length == 1)
                    {
                        // If only the table name is provided, default the schema to "dbo".
                        schema = "dbo";
                        table = parts[0];
                    }
                    else
                    {
                        schema = parts[0].ToLowerInvariant();
                        table = parts[1];
                    }

                    // Determine the display name based on the schema.
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
            )
            .OrderBy(t => t.DisplayName)
            .ToList();

            return View("LinqMvc", model);
        }
    }
}
