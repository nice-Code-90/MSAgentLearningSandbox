using GenerativeAI;
using GenerativeAI.Microsoft;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;

Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.GoogleGeminiApiKey;

string model = GoogleAIModels.Gemini25Flash;

IChatClient client = new GenerativeAIChatClient(apiKey, model);
ChatClientAgent agent = new(client);

string question = "What is the capital of France and how many people live there?";

//Simple
AgentRunResponse response = await agent.RunAsync(question);
Console.WriteLine(response);

Utils.WriteLineDarkGray($"- Input Tokens: {response.Usage?.InputTokenCount}");
Utils.WriteLineDarkGray($"- Output Tokens: {response.Usage?.OutputTokenCount} " +
                        $"({response.Usage?.GetOutputTokensUsedForReasoning()} was used for reasoning)");

//------------------------------------------------------------------------------------------------------------------------
Utils.Separator();

//Streaming
List<AgentRunResponseUpdate> updates = [];
await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync(question))
{
    updates.Add(update);
    Console.Write(update);
}
Console.WriteLine();

AgentRunResponse collectedResponseFromStreaming = updates.ToAgentRunResponse();
Utils.WriteLineDarkGray($"- Input Tokens (Streaming): {collectedResponseFromStreaming.Usage?.InputTokenCount}");
Utils.WriteLineDarkGray($"- Output Tokens (Streaming): {collectedResponseFromStreaming.Usage?.OutputTokenCount} " +
                        $"({collectedResponseFromStreaming.Usage?.GetOutputTokensUsedForReasoning()} was used for reasoning)");

Utils.Separator();
Console.ReadKey();