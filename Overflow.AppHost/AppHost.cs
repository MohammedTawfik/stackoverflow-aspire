using System;

var builder = DistributedApplication.CreateBuilder(args);

var keyCloack = builder.AddKeycloak("keycloack", 6001)
    .WithDataVolume("keycloak-data");

var postgres = builder.AddPostgres(name: "postgres", port: 5432)
    .WithDataVolume("postgres-data")
    .WithPgAdmin();

var questionsDB = postgres.AddDatabase("questions-db");

builder.AddProject<Projects.Questions_API>("questions-api")
    .WithReference(keyCloack)
    .WithReference(questionsDB)
    .WaitFor(keyCloack)
    .WaitFor(questionsDB);


builder.Build().Run();
