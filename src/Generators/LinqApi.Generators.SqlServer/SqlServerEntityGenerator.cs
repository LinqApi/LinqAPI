using LinqApi.Tools.Abstractions;
using LinqApi.Tools.Helpers;
using LinqApi.Tools.Models;

namespace LinqApi.Generators.SqlServer
{
    public class SqlServerEntityGenerator : IEntityGenerator
    {
        private readonly IDatabaseSchemaReader _schemaReader;
        private readonly IEntityConfigurationGenerator _configGenerator;

        public SqlServerEntityGenerator()
        {
            _schemaReader = new SqlServerSchemaReader();
            _configGenerator = new DefaultEntityConfigurationGenerator(); // Bu da aşağıda
        }

        public void Generate(string connectionString, string outputFolder)
        {
            var tables = _schemaReader.ReadTables(connectionString);

            foreach (var table in tables)
            {
                GenerateEntity(table, outputFolder);
                _configGenerator.Generate(table, outputFolder);
            }
        }

        private void GenerateEntity(DatabaseTable table, string outputFolder)
        {
            var className = SchemaNormalizationHelper.Normalize(table.Schema, table.Name);
            var props = string.Join("\n", table.Columns.Select(c =>
                $"public {c.DotNetType.Name}{(c.IsNullable && c.DotNetType.IsValueType ? "?" : "")} {c.Name} {{ get; set; }}"));

            var content = $@"namespace Entities;

public class {className}
{{
{props}
}}";
            File.WriteAllText(Path.Combine(outputFolder, $"{className}.cs"), content);
        }
    }

    public class DefaultEntityConfigurationGenerator : IEntityConfigurationGenerator
    {
        public void Generate(DatabaseTable table, string outputFolder)
        {
            var className = SchemaNormalizationHelper.Normalize(table.Schema, table.Name);
            var content = $@"using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Configurations;

public class {className}Configuration : IEntityTypeConfiguration<{className}>
{{
    public void Configure(EntityTypeBuilder<{className}> builder)
    {{
        builder.ToTable(""{table.Name}"", ""{table.Schema}"");
        builder.HasKey(x => x.{table.PrimaryKey.ColumnName});
    }}
}}";
            File.WriteAllText(Path.Combine(outputFolder, $"{className}Configuration.cs"), content);
        }
    }
}
