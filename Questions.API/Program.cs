using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Questions.API.Data;
using Questions.API.Services;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<TagsService>();
builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer("keycloack", "master", options =>
    {
        options.RequireHttpsMetadata = false;
        options.Audience = "overflow";
    });
// Register the QuestionsDBContext using the Npgsql provider.
// The connection string is resolved from a named connection string "questions-db" or
// from a configuration value with the same key. Adjust as needed for your environment.
var questionsConnection = builder.Configuration.GetConnectionString("questions-db")
                          ?? builder.Configuration["questions-db"]
                          ?? "Host=localhost;Database=questions;Username=postgres;Password=postgres";

builder.Services.AddDbContext<QuestionsDBContext>(options =>
{
    options.UseNpgsql(questionsConnection);
});

builder.Services.AddOpenTelemetry().WithTracing(traceProviderBuilder =>
{
    traceProviderBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService(builder.Environment.ApplicationName))
        .AddSource("Wolverine");
});

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMqUsingNamedConnection("rabbitmq").AutoProvision();
    opts.PublishAllMessages().ToRabbitExchange("questions");
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<QuestionsDBContext>();
    await context.Database.MigrateAsync();
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating or initializing the database.");
}

app.Run();
