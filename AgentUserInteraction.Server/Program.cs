using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using System.ClientModel;
using OpenAI.Chat;
using Microsoft.Extensions.AI;
using OpenAI;

Console.Clear();
Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.CerebrasApiKey;
string modelId = secrets.ModelId;

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClientAgent agent = openAIClient
    .GetChatClient(modelId)
    .AsAIAgent(tools: [AIFunctionFactory.Create(GetWeather, name: "get_weather")]);

//AG-UI Part begin
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddAGUI();
WebApplication app = builder.Build();

app.MapAGUI("/", agent);

await app.RunAsync();

//Server-Tool
static string GetWeather(string city)
{
    return "It is sunny and 19 degrees";
}