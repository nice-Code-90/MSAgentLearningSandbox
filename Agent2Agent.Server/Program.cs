using A2A;
using A2A.AspNetCore;
using Agent2Agent.Server;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using System.ClientModel;
using System.Reflection;
using System.Text;
using OpenAI.Chat;


Console.Clear();
Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.CerebrasApiKey;
string modelId = secrets.ModelId;

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

FileSystemTools target = new();
MethodInfo[] methods = typeof(FileSystemTools).GetMethods(BindingFlags.Public | BindingFlags.Instance);
List<AITool> listOfTools = methods.Select(x => AIFunctionFactory.Create(x, target)).Cast<AITool>().ToList();

AIAgent agent = openAIClient
    .GetChatClient(modelId)
    .AsAIAgent(
        name: "FileAgent",
        instructions: "You are a File Expert. When working with files you need to provide the full path; not just the filename",
        tools: listOfTools
    )
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

AgentCard agentCard = new AgentCard() 
{
    Name = "FilesAgent",
    Description = "Handles requests relating to files",
    Version = "1.0.0",
    DefaultInputModes = ["text"],
    DefaultOutputModes = ["text"],
    Capabilities = new AgentCapabilities()
    {
        Streaming = false,
        PushNotifications = false,
    },
    Skills =
    [
        new AgentSkill()
        {
            Id = "my_files_agent",
            Name = "File Expert",
            Description = "Handles requests relating to files on hard disk",
            Tags = ["files", "folders"],
            Examples = ["What files are the in Folder 'Demo1'"],
        }
    ],
    Url = "https://localhost:54597"
};

app.MapA2A(
    agent,
    path: "/",
    agentCard: agentCard,
    taskManager => app.MapWellKnownAgentCard(taskManager, "/"));

await app.RunAsync();
return;

async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
    }

    Utils.WriteLineDarkGray(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}