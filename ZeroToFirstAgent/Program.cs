using OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using OpenAI.Chat;

Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.CerebrasApiKey;
string modelId = secrets.ModelId;

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClientAgent agent = openAIClient.GetChatClient(modelId).CreateAIAgent();


AgentRunResponse response = await agent.RunAsync("What is the Capital of Australia?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}
Console.WriteLine();