using LinqApi.Controller;
using LinqApi.Core;
using LinqApi.Core.LinqApi.Core;
using LinqApi.Correlation;
using LinqApi.Dynamic.Controller;
using LinqApi.Epoch;
using LinqApi.Localization;
using LinqApi.Localization.Extensions;
using LinqApi.Logging;
using LinqApi.Logging.LinqApi.Core;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

using Posonl.Infrastructure;
using System.Text.Json.Serialization;

namespace Posonl.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            IServiceCollection services = builder.Services;

            // Localization settings
            _ = services.AddLocalization();
            _ = services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = true;
            })
                .AddViewLocalization()
                .AddDataAnnotationsLocalization()
    // 2. ApplicationPartManager konfigürasyonu
    .ConfigureApplicationPartManager(apm =>
    {
        // 3. Seçenek nesnesini oluştur

        apm.FeatureProviders.Add(new LinqApiControllerFeatureProvider(new LinqApiApmOptions
        {
            DbContextType = typeof(PosOnlDbContext),
            //AreaName = "DynamicArea", // İsterseniz
            ControllerType = LinqControllerType.LinqController
        }));

        apm.FeatureProviders.Add(new LinqApiControllerFeatureProvider(new LinqApiApmOptions
        {
            DbContextType = typeof(LinqLoggingDbContext),
            AreaName = "LinqLogs", // İsterseniz
            ControllerType = LinqControllerType.LinqController
        }));


        LinqApi.Razor.MvcHelpers.CreateViews(apm);
    });

            services.AddHttpContextAccessor();

            // Şema parametresini sağlamak için DbContextOptionsBuilder'ı kullanın
            _ = services.AddScoped(provider =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<LinqLocalizationDbContext>();
                    _ = optionsBuilder.UseSqlServer(configuration.GetConnectionString("PosOnlDb"));

                    //adds linqlog for db via interceptor from linq
                    _ = optionsBuilder.AddInterceptors(provider.GetRequiredService<LinqSqlLoggingInterceptor>());

                    // İstediğiniz şema adını burada belirtin
                    return new PosOnlDbContext(optionsBuilder.Options, "posonl"); // Şema adını burada verin
                });


            #region linqLogRelated

            _ = services.AddScoped<ILinqLogger, LinqDbContextCallLogger>();
            _ = services.AddScoped<LinqSqlLoggingInterceptor>();
            _ = services.AddDefaultLinqEpochProvider();
            // Register logging DbContext (and outbox table if needed)
            _ = services.AddDbContext<LinqLoggingDbContext>(options =>
            {
                _ = options.UseSqlServer(configuration.GetConnectionString("logdb"));
                // Optionally add the SQL interceptor:
            });

            #endregion

            // Register main application DbContext
            _ = services.AddDbContext<PosOnlDbContext>((serviceProvider, options) =>
            {
                var interceptor = serviceProvider.GetRequiredService<LinqSqlLoggingInterceptor>();
                _ = options.UseSqlServer(configuration.GetConnectionString("PosOnlDb"))
                       .AddInterceptors(interceptor);
            }, ServiceLifetime.Scoped, ServiceLifetime.Scoped); //Ekledim ki scope'a gore alsin



            _ = services.AddEndpointsApiExplorer();
            _ = services.AddSwaggerGen();

            _ = services.AddRepositoriesForDbContext<PosOnlDbContext>();
            _ = services.AddRepositoriesForDbContext<LinqLoggingDbContext>();

            // Register logging services and options
            _ = services.Configure<LinqLoggingOptions>(configuration.GetSection("LinqLogging"));
            _ = services.AddSingleton<ILinqPayloadMasker, DefaultPayloadMasker>();
            _ = services.AddSingleton<ICorrelationIdGenerator, DefaultCorrelationIdGenerator>();
            _ = services.AddSingleton<IEpochProvider, DefaultLinqEpochProvider>();
            //Register MassTransit consumers and configuration for AWS SQS outbox integration.
            // LinqLoggingObserver'ı singleton olarak kaydediyoruz.
            //_ = services.AddSingleton<LinqLoggingObserver>();
            // Register HTTP Client with logging delegating handler
            _ = services.AddTransient<LinqHttpDelegatingHandler>();
            // ILinqLogger ve diğer bağımlılıklarınız scoped veya singleton olabilir:
            _ = services.AddScoped<ILinqLogger, LinqDbContextCallLogger>();


            _ = services.AddScoped<IUserContext<string>, LinqAnonymousUserContext>();

            _ = services.AddHttpClient("LoggedClient")
                .AddHttpMessageHandler<LinqHttpDelegatingHandler>();

            var app = builder.Build();
            _ = app.UseMiddleware<LinqHttpLoggingMiddleware>();
            if (!app.Environment.IsDevelopment())
            {
                _ = app.UseExceptionHandler("/Home/Error");
            }
            var supportedCultures = new[] { "tr-TR", "en-US", "de-DE" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture("tr-TR")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            // Enable cookie-based culture selection.
            localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());

            _ = app.UseRequestLocalization(localizationOptions);
            _ = app.UseStaticFiles();
            _ = app.UseRouting();

            // Apply migrations for the main application DB.
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PosOnlDbContext>();
                dbContext.Database.Migrate();
            }

            // Apply migrations for the logging DB.
            using (var scope = app.Services.CreateScope())
            {
                var loggingDbContext = scope.ServiceProvider.GetRequiredService<LinqLoggingDbContext>();
                loggingDbContext.Database.Migrate();
            }



            if (app.Environment.IsDevelopment())
            {
                _ = app.UseSwagger();
                _ = app.UseSwaggerUI();
            }

            // Localized route: language parameter is required in the URL.
            _ = app.MapControllerRoute(
                name: "localized",
                pattern: "{lang:regex(^tr$|^en$|^de$)}/{controller=Home}/{action=Index}/{id?}"
            );

            // Area route for Dashboard controllers
            _ = app.MapAreaControllerRoute(
                name: "Dashboard",
                areaName: "Dashboard",
                pattern: "Dashboard/{controller=Home}/{action=Index}/{id?}"
            );

            // Default route for non-area controllers; force area to be empty.
            _ = app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}",
                defaults: new { area = "" }
            );

            app.Run();
        }
    }
}
