using OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using Shared.Extensions;

Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.CerebrasApiKey;
string modelId = "llama-3.3-70b";

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

IChatClient client = openAIClient.GetChatClient(modelId).AsIChatClient();
ChatClientAgent agent = new(client);

string question = "What is the capital of France and how many people live there?";

// --- SIMPLE RUN ---
AgentRunResponse response = await agent.RunAsync(question);
Console.WriteLine(response);

response.Usage.OutputAsInformation();

//------------------------------------------------------------------------------------------------------------------------
Utils.Separator();

// --- STREAMING RUN ---
List<AgentRunResponseUpdate> updates = [];
await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync(question))
{
    updates.Add(update);
    Console.Write(update);
}
Console.WriteLine();

AgentRunResponse collectedResponseFromStreaming = updates.ToAgentRunResponse();
collectedResponseFromStreaming.Usage.OutputAsInformation();

Utils.Separator();
Console.ReadKey();