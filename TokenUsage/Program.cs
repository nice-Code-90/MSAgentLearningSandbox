using OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using Shared.Extensions;
using OpenAI.Chat;

Secrets secrets = SecretManager.GetSecrets();

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClientAgent agent = openAIClient
    .GetChatClient(secrets.ModelId)
    .AsAIAgent(instructions: "You are a helpful assistant.");

string question = "What is the capital of France and how many people live there?";

AgentResponse response = await agent.RunAsync(question);
Console.WriteLine(response);

response.Usage.OutputAsInformation();

Utils.Separator();
List<AgentResponseUpdate> updates = [];
await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(question))
{
    updates.Add(update);
    Console.Write(update);
}
Console.WriteLine();

AgentResponse collectedResponseFromStreaming = updates.ToAgentResponse();
collectedResponseFromStreaming.Usage.OutputAsInformation();

Utils.Separator();
Console.ReadKey();