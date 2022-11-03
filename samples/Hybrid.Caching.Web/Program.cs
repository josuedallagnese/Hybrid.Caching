using System.Text.Json.Serialization;
using Hybrid.Caching;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(config =>
    {
        config.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHybridCaching(builder.Configuration);

var app = builder.Build();

app.UseHybridCaching();

app.UseRouting();

app.UseEndpoints(options =>
{
    options.MapControllers();
});

app.UseSwagger(c =>
{
    c.SerializeAsV2 = true;
});

app.UseSwaggerUI();

app.Run();
