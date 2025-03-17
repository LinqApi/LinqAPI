using LinqApi.Tools.Models;

namespace LinqApi.Tools.Abstractions
{
    public interface IEntityConfigurationGenerator
    {
        void Generate(DatabaseTable table, string outputFolder);
    }
}
