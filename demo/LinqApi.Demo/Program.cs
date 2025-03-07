using LinqApi.Helpers;
using LinqApi.Razor;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Swagger eklemeleri (opsiyonel)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dinamik API controller'larınızı ekleyin
//builder.Services.AddLinqMsApi("tiriviri","Data Source=TSTLSNSQL;Database=testmoka_db;User ID=DevMokaUser;Password=MokaPass2532;TrustServerCertificate=Yes");

builder.Services.AddLinqMsApi("");

// MVC'yi view desteğiyle birlikte ekleyin
builder.Services.AddControllersWithViews()
    .ConfigureApplicationPartManager(apm =>
    {
        // LinqApi içerisindeki dinamik controller'ları ekleyelim:
        var dynamicProvider = builder.Services.BuildServiceProvider().GetRequiredService<DynamicLinqApiControllerFeatureProvider>();
        apm.FeatureProviders.Add(dynamicProvider);

        // RCL (Razor Class Library) assembly'sini de ApplicationPart olarak ekleyin.
        // Örneğin, RCL içerisindeki herhangi bir tipin Assembly'sini kullanabilirsiniz:
        var razorAssembly = typeof(RclMarker).GetTypeInfo().Assembly;
        apm.ApplicationParts.Add(new AssemblyPart(razorAssembly));
    });

var app = builder.Build();

// Geliştirme ortamında Swagger kullanımı
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Statik dosyaları sunmak (CSS, JS, img vs.)
app.UseStaticFiles();

app.UseRouting();

// Varsayılan route'u ayarlıyoruz, örneğin MSController'daki Index action'ı çalışacak.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LinqMS}/{action=Index}/{id?}");

app.Run();
