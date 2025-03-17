
using LinqApi.Dynamic.Controller;
using LinqApi.Dynamic.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDynamicLinqApi("api", "Data Source=.\\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;TrustServerCertificate=Yes");



// MVC'yi view desteðiyle birlikte ekleyin
builder.Services.AddControllersWithViews()
    .ConfigureApplicationPartManager(apm =>
    {
        // LinqApi içerisindeki dinamik controller'larý ekleyelim:
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

// Statik dosyalarý sunmak (CSS, JS, img vs.)
app.UseStaticFiles();

app.UseRouting();

// Varsayýlan route'u ayarlýyoruz, örneðin MSController'daki Index action'ý çalýþacak.
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=LinqMvc}/{action=Index}/{id?}");


app.MapDefaultControllerRoute();
app.Run();
