
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SearchService.Data;
using SearchService.Models;
using System.Text.RegularExpressions;
using Typesense;
using Typesense.Setup;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var typeSenseUri = builder.Configuration.GetValue<string>("services:typesense:typeSense-endpoint:0");
if (string.IsNullOrEmpty(typeSenseUri))
{
    throw new InvalidOperationException("TypeSense endpoint is not configured.");
}
var uri = new Uri(typeSenseUri);

var typeSeseApiKey = builder.Configuration.GetValue<string>("typesene-api-key");
if (string.IsNullOrEmpty(typeSeseApiKey))
{
    throw new InvalidOperationException("TypeSense API key is not configured.");
}
builder.Services.AddTypesenseClient(options =>
{
    options.ApiKey = typeSeseApiKey;
    options.Nodes = new List<Node>
    {
         new Node(
            uri.Host,
            uri.Port.ToString(),
            uri.Scheme
        )
    };
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
    opts.ListenToRabbitQueue("questions.search", config => { 
        config.BindExchange("questions");
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

var scope = app.Services.CreateScope();
var client = scope.ServiceProvider.GetRequiredService<ITypesenseClient>();
await SearchInitializer.EnsureIndexExists(client);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/search", async (string query, ITypesenseClient client) =>
{
    string? tags = null;
    var tagsMatch = Regex.Match(query, @"\[(.*?)\]");
    if (tagsMatch.Success)
    {
        tags = tagsMatch.Groups[1].Value;
        query = query.Replace(tagsMatch.Value, "").Trim();
    }
    var searchParams = new SearchParameters(query, "title,content");
    if (!string.IsNullOrWhiteSpace(tags))
    {
        searchParams.FilterBy = $"tags:[{tags}]";
    }
    try
    {
        var results = await client.Search<QuestionSearchInfo>("questions", searchParams);
        return Results.Ok(results.Hits.Select(hit => hit.Document));
    }
    catch (Exception ex)
    {
        return Results.Problem("An error occurred while searching for questions.",ex.Message);
    }
});

app.MapGet("/search/similar-titles", async (string query, ITypesenseClient typesenseClient) => 
{
    try
    {
        var searchParams = new SearchParameters(query, "title");
        var result = await typesenseClient.Search<QuestionSearchInfo>("questions", searchParams);
        return Results.Ok(result.Hits.Select(hit => hit.Document));
    }
    catch (Exception ex)
    {
        return Results.Problem($"An Error occcured while searching for {query}",ex.Message);
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
