using System;

var builder = DistributedApplication.CreateBuilder(args);

var keyCloack = builder.AddKeycloak("keycloack", 6001)
    .WithDataVolume("keycloak-data");

builder.Build().Run();
