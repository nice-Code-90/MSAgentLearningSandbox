
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AzureFunctions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using OpenAI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;

//Start storage docker
// docker run -d --name storage-emulator -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

Secrets secrets = SecretManager.GetSecrets();

string apiKey = secrets.CerebrasApiKey;
string modelId = secrets.ModelId;
var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClientAgent myAgent = openAIClient
    .GetChatClient(secrets.ModelId)
    .AsAIAgent(name: "MyAgent");


//From nuget: Microsoft.Agents.AI.Hosting.AzureFunctions
builder.ConfigureDurableAgents(options => options.AddAIAgent(myAgent));

builder.Build().Run();