using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LinqApi.PowerTools
{
    public static class MyDesignTimeServiceCollectionExtensions
    {
        public static IServiceCollection AddMyEfCoreDesignTimeServices(
            this IServiceCollection services)
        {
            // Örnek: CSharpHelper, AnnotationCodeGenerator vb.
            services.TryAddSingleton<ICSharpHelper, CSharpHelper>();
            services.TryAddSingleton<IAnnotationCodeGenerator, CSharpAnnotationCodeGenerator>();

            // C# kod üreticileri (DbContext, EntityType) – EF Core 5/6/7'de isimler değişebilir
            services.TryAddSingleton<ICSharpDbContextGenerator, CSharpDbContextGenerator>();
            services.TryAddSingleton<ICSharpEntityTypeGenerator, CSharpEntityTypeGenerator>();

            // IModelCodeGenerator -> CSharpModelGenerator
            services.TryAddSingleton<IModelCodeGenerator, CSharpModelGenerator>();

            // Scaffolding – relational
            services.TryAddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>();
            services.TryAddSingleton<IProviderConfigurationCodeGenerator, ProviderConfigurationCodeGenerator>();

            // Belki .TryAddSingleton<IPluralizer, ...>(); (eğer pluralization varsa)
            // vs. opsiyonel

            return services;
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            ScaffoldMyDb("Server=.\\SQLEXPRESS;Database=ppp4;Trusted_Connection=True;TrustServerCertificate=True;", ".", "TirtX");
            Console.WriteLine("Hello, World!");
        }


        public static void ScaffoldMyDb(string connectionString, string outputDir, string ns)
        {
            // 1) ServiceProvider
            var services = new ServiceCollection();
            services.AddMyEfCoreDesignTimeServices();
            // Provider design-time service reflection ekleyin (SqlServer, MySQL, vs.)
            var sp = services.BuildServiceProvider();

            // 2) DB -> IModel
            var scaffoldingFactory = sp.GetService<IScaffoldingModelFactory>();
            var model = scaffoldingFactory.Create(connectionString, new DatabaseModelFactoryOptions());

            // 3) IModel -> C# Code (ScaffoldedModel)
            var codeGenerator = sp.GetService<IModelCodeGenerator>();
            var genOptions = new ModelCodeGenerationOptions
            {
                ModelNamespace = ns,
                ContextNamespace = ns,
                // vs...
            };
            var scaffolded = codeGenerator.GenerateModel(model, genOptions);

            // 4) Kod Dosyalarını Yazma 
            // (veya manipüle etme, T4’e vs.)
            // scaffolded.ContextFile.Code, scaffolded.AdditionalFiles
            // O "3-5 satır" ekleme ya da T4'e parametre verme vs.
            // En basit: Disk'e kaydediyoruz:
            if (scaffolded.ContextFile != null)
            {
                File.WriteAllText(Path.Combine(outputDir, scaffolded.ContextFile.Path),
                                  scaffolded.ContextFile.Code);
            }
            foreach (var file in scaffolded.AdditionalFiles)
            {
                File.WriteAllText(Path.Combine(outputDir, file.Path),
                                  file.Code);
            }
        }

    }
}
