using LinqApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Swagger eklemeleri (opsiyonel)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string connectionString = "";

builder.Services.AddLinqApi("api", connectionString);
//builder.Services.AddLinqApi("api", connectionString);

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
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=LinqMvc}/{action=Index}/{id?}");


app.MapDefaultControllerRoute();
app.Run();

