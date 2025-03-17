namespace LinqApi.Tools.Abstractions
{
    public interface IEntityGenerator
    {
        void Generate(string connectionString, string outputFolder);
    }
}
