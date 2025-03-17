using LinqApi.Tools.Models;

namespace LinqApi.Tools.Abstractions
{
    public interface IDatabaseSchemaReader
    {
        IEnumerable<DatabaseTable> ReadTables(string connectionString);
    }
}
