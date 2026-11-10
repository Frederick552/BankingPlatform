using System.Text.Json.Serialization;
using Banking.Api.ErrorHandling;
using Banking.Application;
using Banking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Banking")
    ?? "Data Source=banking.db";

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // /openapi/v1.json
}

app.MapControllers();

app.Run();

// Necesario para que los tests de integración (WebApplicationFactory<Program>) puedan referenciar el punto de entrada.
public partial class Program { }
