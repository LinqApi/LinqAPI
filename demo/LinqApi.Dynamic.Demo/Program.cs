
using LinqApi.Dynamic.Controller;
using LinqApi.Dynamic.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDynamicLinqApi(string.Empty, "Data Source=.\\SQLEXPRESS;Database=ppp4;Trusted_Connection=True;TrustServerCertificate=Yes");

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

// Configure the HTTP request pipeline.
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


_ = app.MapControllerRoute(
               name: "default",
               pattern: "{controller=Home}/{action=Index}/{id?}",
               defaults: new { area = "" }
           );
app.Run();
