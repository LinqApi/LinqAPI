namespace LinqApi.Logging
{
    /// <summary>
    /// Represents pagination input parameters without internal validation.
    /// Consumers are responsible for validating values as needed.
    /// </summary>
    public class Pager
    {
        /// <summary>
        /// Gets or sets the page number (starting from 1). No validation is applied.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page. No max/min constraint is applied here.
        /// </summary>
        public int PageSize { get; set; }
    }
}
