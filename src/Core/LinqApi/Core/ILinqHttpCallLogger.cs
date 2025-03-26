using Microsoft.Extensions.Logging;

namespace LinqApi.Core
{
    /// <summary>
    /// Interface for logging HTTP call information.
    /// </summary>
    public interface ILinqHttpCallLogger : ILogger
    {
        Task LogAsync(LinqHttpCallLog log);
    }


}

