using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = DistributedApplication.CreateBuilder(args);

var keyCloack = builder.AddKeycloak("keycloack", 6001)
    .WithDataVolume("keycloak-data");

var postgres = builder.AddPostgres(name: "postgres", port: 5432)
    .WithDataVolume("postgres-data")
    .WithPgAdmin();

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithDataVolume("rabbitmq-data")
    .WithManagementPlugin(port:15672);

var questionsDB = postgres.AddDatabase("questions-db");

var typeSeseApiKey = builder.AddParameter("typesense-api-key", secret: true);

var typeSense = builder.AddContainer("typesense", "typesense/typesense", "29.0")
    .WithArgs("--data-dir", "/data", "--api-key", typeSeseApiKey, "--enable-cors")
    .WithVolume("typesense-data", "/data")
    .WithHttpEndpoint(8108, 8108, name: "typeSense-endpoint");

var typeSenseCotainer = typeSense.GetEndpoint("typeSense-endpoint");

var questionsService = builder.AddProject<Projects.Questions_API>("questions-api")
    .WithReference(keyCloack)
    .WithReference(questionsDB)
    .WithReference(rabbitMq)
    .WaitFor(keyCloack)
    .WaitFor(questionsDB)
    .WaitFor(rabbitMq);


var searchService = builder.AddProject<Projects.SearchService>("searchservice")
    .WithEnvironment("typesene-api-key", typeSeseApiKey)
    .WithReference(typeSenseCotainer)
    .WithReference(rabbitMq)
    .WaitFor(typeSense)
    .WaitFor(rabbitMq);


builder.Build().Run();
