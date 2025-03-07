using LinqApi.Helpers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddDbContext<DemoDbContext>(options =>
//    options.UseInMemoryDatabase("DemoDb"));

////just to create in memory mock data
//using (var scope = builder.Services.BuildServiceProvider().CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
//    dbContext.Database.EnsureCreated(); // Ensure database is created
//}


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLinqMsApi("Data Source=TSTLSNSQL;Database=testmoka_db;User ID=DevMokaUser;Password=MokaPass2532;TrustServerCertificate=Yes");

builder.Services.AddControllers()
    .ConfigureApplicationPartManager(apm =>
    {
        var provider = builder.Services.BuildServiceProvider().GetRequiredService<DynamicLinqmsControllerFeatureProvider>();
        apm.FeatureProviders.Add(provider);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
