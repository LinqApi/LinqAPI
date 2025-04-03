using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LinqApi.Core
{
    /// <summary>
    /// Represents the filtering, ordering, grouping, selecting, and paging logic for dynamic LINQ queries.
    /// </summary>
    public class LinqFilterModel
    {
        /// <summary>
        /// Gets or sets the filter expression in string format. 
        /// Example: <c>"Age > 30 and Name.StartsWith(\"A\")"</c>
        /// </summary>
        /// 
        [AllowNull]
        public string? Filter { get; set; }

        /// <summary>
        /// Gets or sets the pagination configuration for the main dataset.
        /// </summary>
        public Pager Pager { get; set; }

        /// <summary>
        /// Gets or sets the field to order by. 
        /// Example: <c>"Name"</c>
        /// </summary>
        /// 
        [AllowNull]
        public string? OrderBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ordering should be descending.
        /// </summary>
        public bool Desc { get; set; }

        /// <summary>
        /// Gets or sets the grouping logic in string format.
        /// Example: <c>"new (DepartmentId)"</c>
        /// </summary>
        /// 

        public string? GroupBy { get; set; }

        /// <summary>
        /// Gets or sets the select/projection fields.
        /// Example: <c>"new (Name, Age, Salary.Max())"</c>
        /// </summary>
        /// 
        [AllowNull]
        public string? Select { get; set; }

        /// <summary>
        /// Gets or sets the navigation properties to be included in the query.
        /// </summary>
        public List<IncludeModel> Includes { get; set; } = new();
    }

    /// <summary>
    /// Represents an Include path used for eager loading in LINQ.
    /// </summary>
    public class IncludeModel
    {
        /// <summary>
        /// Gets or sets the name of the navigation property to include.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the pagination for the included property (optional).
        /// </summary>
        public Pager? Pager { get; set; }

        /// <summary>
        /// Gets or sets the nested ThenIncludes for deeper navigation trees.
        /// </summary>
        public List<ThenIncludeModel> ThenIncludes { get; set; } = new();
    }

    /// <summary>
    /// Represents a ThenInclude step for nested navigation property inclusion.
    /// </summary>
    public class ThenIncludeModel
    {
        /// <summary>
        /// Gets or sets the parent property that contains the child includes.
        /// </summary>
        public string ParentProperty { get; set; }

        /// <summary>
        /// Gets or sets the list of child includes under the parent navigation.
        /// </summary>
        public List<IncludeModel> ChildIncludes { get; set; } = new();
    }
}
