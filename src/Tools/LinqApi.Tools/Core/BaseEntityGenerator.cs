using LinqApi.Tools.Abstractions;
using LinqApi.Tools.Helpers;
using LinqApi.Tools.Models;

namespace LinqApi.Tools.Core
{
    public abstract class BaseEntityGenerator : IEntityGenerator
    {
        protected IDatabaseSchemaReader SchemaReader;
        protected IEntityConfigurationGenerator ConfigurationGenerator;

        protected BaseEntityGenerator(IDatabaseSchemaReader schemaReader, IEntityConfigurationGenerator configGenerator)
        {
            SchemaReader = schemaReader;
            ConfigurationGenerator = configGenerator;
        }

        public virtual void Generate(string connectionString, string outputFolder)
        {
            var tables = SchemaReader.ReadTables(connectionString);
            foreach (var table in tables)
            {
                GenerateEntity(table, outputFolder);
                ConfigurationGenerator.Generate(table, outputFolder);
            }
        }

        protected abstract void GenerateEntity(DatabaseTable table, string outputFolder);
    }


}
