var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.AgentUserInteraction_Advanced_Server>("server");

builder.AddProject<Projects.AgentUserInteraction_Advanced_BlazorWasmClient>("blazor-wasm-client")
    .WithReference(server);

builder.Build().Run();
