namespace LinqApi.Logging
{
    /// <summary>
    /// Represents a paginated result set.
    /// </summary>
    /// <typeparam name="T">The type of the data items.</typeparam>
    public class PaginationModel<T>
    {
        /// <summary>
        /// Gets or sets the list of items returned in the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the total number of records matching the query (before paging).
        /// </summary>
        public int TotalRecords { get; set; }
    }
}
