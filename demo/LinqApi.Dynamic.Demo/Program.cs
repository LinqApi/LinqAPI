
using LinqApi.Dynamic.Controller;
using LinqApi.Dynamic.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDynamicLinqApi("api", "Data Source=.\\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;TrustServerCertificate=Yes");



// MVC'yi view deste�iyle birlikte ekleyin
builder.Services.AddControllersWithViews()
    .ConfigureApplicationPartManager(apm =>
    {
        // LinqApi i�erisindeki dinamik controller'lar� ekleyelim:
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

// Statik dosyalar� sunmak (CSS, JS, img vs.)
app.UseStaticFiles();

app.UseRouting();

// Varsay�lan route'u ayarl�yoruz, �rne�in MSController'daki Index action'� �al��acak.
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=LinqMvc}/{action=Index}/{id?}");


app.MapDefaultControllerRoute();
app.Run();
