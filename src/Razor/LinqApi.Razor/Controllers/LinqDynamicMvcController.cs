using LinqApi.Dynamic;
using Microsoft.AspNetCore.Mvc;

namespace LinqApi.Razor
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
    public class LinqDynamicMvcController : Microsoft.AspNetCore.Mvc.Controller
    {
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


            return View("Index");
        }

        public virtual IActionResult Select2()

        {
            return View("Select2");
        }
    }
}
