using LinqApi.Demo.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Swagger eklemeleri (opsiyonel)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// InMemory bir veritabanı için farklı alanlara özel (area bazlı) DBContext tanımları:
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseInMemoryDatabase("ApiDatabase"));

builder.Services.AddRepositoriesFromAssembly<ApiDbContext>();

builder.Services.AddControllersWithViews();


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

// Varsayılan route'u ayarlıyoruz
app.MapDefaultControllerRoute();
app.Run();