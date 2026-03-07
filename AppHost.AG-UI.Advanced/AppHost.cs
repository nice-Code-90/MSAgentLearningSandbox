var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.AgentUserInteraction_Advanced_Server>("server");

builder.AddProject<Projects.AgentUserInteraction_Advanced_BlazorWasmClient>("blazor-wasm-client")
    .WithReference(server);

builder.AddAzureFunctionsProject<Projects.FunctionApp_HelloAI>("functionapp-helloai");

builder.Build().Run();
