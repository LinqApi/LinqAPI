using LinqApi.Helpers;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLinqMsApi("");

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
