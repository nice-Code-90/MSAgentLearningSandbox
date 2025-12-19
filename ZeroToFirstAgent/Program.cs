/* Steps:
 * 1: Get a Google API Gemini API Key (https://aistudio.google.com/app/api-keys)
 * 2: Add Nuget Packages (Google_GenerativeAI.Microsoft + Microsoft.Agents.AI)
 * 3: Create an GenerativeAIChatClient for an ChatClientAgent
 * 4: Call RunAsync or RunStreamingAsync
 */
using OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;

Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.CerebrasApiKey;
string modelId = "llama-3.3-70b";
var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);



IChatClient client = openAIClient.GetChatClient(modelId).AsIChatClient();
ChatClientAgent agent = new(client);
AgentRunResponse response = await agent.RunAsync("What is the Capital of Australia?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}
Console.WriteLine();