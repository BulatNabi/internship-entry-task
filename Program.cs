using Microsoft.EntityFrameworkCore;
using TestTaskModulBank.Data;
using TestTaskModulBank.Interfaces;
using TestTaskModulBank.Repositories;
using TestTaskModulBank.Services;
using System.Text.Json; 
using System.Text.Json.Serialization; 
using TestTaskModulBank.Converters; 

var builder = WebApplication.CreateBuilder(args);

var jsonSerializerOptions = new JsonSerializerOptions
{
    Converters = { new PlayerSymbolArrayConverter(), new JsonStringEnumConverter() },
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
    WriteIndented = builder.Environment.IsDevelopment() 
};
builder.Services.AddSingleton(jsonSerializerOptions); 

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = jsonSerializerOptions.PropertyNameCaseInsensitive;
        options.JsonSerializerOptions.PropertyNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy;
        options.JsonSerializerOptions.WriteIndented = jsonSerializerOptions.WriteIndented;
        foreach (var converter in jsonSerializerOptions.Converters)
        {
            options.JsonSerializerOptions.Converters.Add(converter);
        }
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>(provider =>
    new GameService(
        provider.GetRequiredService<IGameRepository>(),
        provider.GetRequiredService<IConfiguration>(),
        provider.GetRequiredService<JsonSerializerOptions>() 
    ));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();