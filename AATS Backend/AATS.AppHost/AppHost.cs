var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.AATS_API>("api")
    .WithExternalHttpEndpoints();

var server = builder.AddProject<Projects.AATS_Server>("server")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../../AATS Frontend/frontend")
    .WithReference(api)
    .WithReference(server)
    .WaitFor(api);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();
