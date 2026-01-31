#pragma warning disable OPENAI001

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;
using OpenAI.Chat;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using Microsoft.Extensions.Logging;

Secrets secrets = SecretManager.GetSecrets();

OpenAIClient cerebrasClient = new(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClient chatClient = cerebrasClient.GetChatClient(secrets.ModelId);

ChatClientAgent noSettingAgent = chatClient.AsIChatClient().CreateCerebrasAgent();
ChatClientAgent surferAgent = chatClient.AsIChatClient().CreateCerebrasAgent(  
    instructions: "You are a cool surfer dude",
    name: "SurferAgent"
);

#region Set all option parameters (Middleware and DI)

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton(new MySpecialService());
ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

string sourceName = Guid.NewGuid().ToString("N");
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(sourceName)
    .AddConsoleExporter()
    .Build();


AIAgent agentWithAllSettings = chatClient.AsIChatClient().CreateCerebrasAgent(
        instructions: "Speak like a Pirate",
        name: "CerebrasPirateAgent",
        description: "A Cerebras-powered pirate agent demonstrating full configuration",
        tools: [],
        loggerFactory: LoggerFactory.Create(lb => lb.AddConsole()),
        services: serviceProvider
    )
    .AsBuilder()
    .UseOpenTelemetry(sourceName)
    .Build();

// 6. Execute and Display
var response = await agentWithAllSettings.RunAsync("What is the capital of France?");

Console.WriteLine("Raw Response:");
Utils.WriteLineDarkGray(response.ToString());

Utils.Separator();

Console.WriteLine("Cleaned Content:");
Utils.WriteLineGreen(response.GetCleanContent());

#endregion

#region Even more options via ChatClientAgentOptions

ChatClientAgent advancedAgent = new ChatClientAgent(
    chatClient.AsIChatClient(),
    new ChatClientAgentOptions
    {
        Name = "AdvancedCerebras",
        Description = "Direct configuration using ChatClientAgentOptions",
        ChatOptions = new ChatOptions
        {
            Instructions = "Speak like a scientist",
            Temperature = 0.1f 
        }
    },
    LoggerFactory.Create(lb => lb.AddConsole()),
    serviceProvider
);

#endregion

public class MySpecialService
{
    public void DoSomething() => Console.WriteLine("Special Service is executing its task...");
}