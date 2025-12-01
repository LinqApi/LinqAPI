using LinqApi.Core.Log;
using Microsoft.Extensions.Logging;

namespace LinqApi.Core
{
    /// <summary>
    /// Interface for logging HTTP call information.
    /// </summary>
    public interface ILinqLogger : ILogger
    {
        Task LogAsync(LinqLogEntity log,CancellationToken cancellationToken);
    }
}