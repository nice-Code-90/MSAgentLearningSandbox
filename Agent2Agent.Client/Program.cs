using A2A;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;

Utils.WriteLineDarkGray("Initializing");
Utils.WriteLineDarkGray("- Waiting 1 sec for the server to be ready");
await Task.Delay(1000);
Secrets secrets = SecretManager.GetSecrets();

string apiKey = secrets.CerebrasApiKey;
string modelId = secrets.ModelId;

Utils.WriteLineDarkGray("- Connecting to Remote Agent");
A2ACardResolver agentCardResolver = new A2ACardResolver(new Uri("https://localhost:54597/"));
AIAgent remoteAgent = await agentCardResolver.GetAIAgentAsync();

Utils.Separator();

Utils.WriteLineDarkGray("Ready for questions");
var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClientAgent agent = openAIClient
    .GetChatClient(modelId)
    .AsAIAgent(
        name: "ClientAgent",
        instructions: "You specialize in handling queries for users and using your tools to provide answers.",
        tools: [remoteAgent.AsAIFunction()]);

AgentSession session = await agent.CreateSessionAsync();
while (true)
{
    Console.Write("> ");
    string message = Console.ReadLine() ?? string.Empty;
    if (message == string.Empty)
    {
        continue;
    }

    AgentResponse response = await agent.RunAsync(message, session);
    Console.WriteLine(response);
}