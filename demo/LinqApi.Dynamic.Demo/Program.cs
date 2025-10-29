
using LinqApi.Dynamic.Controller;
using LinqApi.Dynamic.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDynamicLinqApi(string.Empty, "Server=188.132.201.149;Database=eventiggcik100;User Id=sa;Password=f0rmAndAter0lmAyAgeld!k.;TrustServerCertificate=True;Connection Timeout=1500;");

builder.Services.AddHttpContextAccessor();



// MVC'yi view desteğiyle birlikte ekleyin
builder.Services.AddControllersWithViews()
    .ConfigureApplicationPartManager(apm =>
    {
        // LinqApi içerisindeki dinamik controller'ları ekleyelim:
        var dynamicProvider = builder.Services.BuildServiceProvider().GetRequiredService<DynamicLinqApiControllerFeatureProvider>();
        apm.FeatureProviders.Add(dynamicProvider);

        LinqApi.Razor.MvcHelpers.CreateViews(apm);
    });


var app = builder.Build();

app.UseRouting();

app.UseStaticFiles();

_ = app.MapControllerRoute(
               name: "default",
               pattern: "{controller=LinqDynamicMvc}/{action=Index}/{id?}",
               defaults: new { area = "" }
           );
app.Run();
