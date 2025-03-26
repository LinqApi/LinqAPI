using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace LinqApi.Core
{
    public class SqlLoggingInterceptor : DbCommandInterceptor
    {
        // You can inject a logger (or the ILinqHttpCallLogger) if you wish.

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) => base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);


        // Similarly override NonQueryExecutingAsync and ScalarExecutingAsync if needed.
    }


}

