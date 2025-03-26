namespace LinqApi.Core
{
    /// <summary>
    /// Holds the correlation information for the current scope.
    /// </summary>
    public class CorrelationInfo
    {
        public CorrelationId ParentId { get; set; }
        public int Step { get; set; }
    }


}

