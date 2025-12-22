#pragma warning disable MEAI001
using OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using Shared.Extensions;

// 1. Setup Cerebras Secrets
Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.CerebrasApiKey;
string modelId = secrets.ModelId;

// 2. Initialize Cerebras Client with high timeout for reasoning
var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri("https://api.cerebras.ai/v1"),
        NetworkTimeout = TimeSpan.FromMinutes(5)
    }
);

IChatClient GetCerebrasClient(string model) => openAIClient
    .GetChatClient(model)
    .AsIChatClient();

string question = "What is the Capital of France and how many people live there?";

// --- APPROACH 1: Default Reasoning ---
ChatClientAgent agentDefault = GetCerebrasClient(modelId).CreateCerebrasAgent();

Console.WriteLine("--- Approach 1: Default Reasoning ---");
AgentRunResponse response1 = await agentDefault.RunAsync(question);

string rawOutput1 = response1.ToString();
string cleanedOutput1 = rawOutput1;
if (cleanedOutput1.Contains("</think>")) cleanedOutput1 = cleanedOutput1.Split("</think>").Last().Trim();

Console.WriteLine(cleanedOutput1);
response1.Usage.OutputAsInformation(rawOutput1);
Utils.Separator();

// --- APPROACH 2: Controlling Reasoning via Max Tokens ---
// Simplified: Just pass the maxTokens parameter directly
ChatClientAgent agentLimitedReasoning = GetCerebrasClient(modelId)
    .CreateCerebrasAgent(maxTokens: 500);

Console.WriteLine("--- Approach 2: Limited Reasoning (Token Control) ---");
AgentRunResponse response2 = await agentLimitedReasoning.RunAsync(question);

string rawOutput2 = response2.ToString();
string cleanedOutput2 = rawOutput2;
if (cleanedOutput2.Contains("</think>")) cleanedOutput2 = cleanedOutput2.Split("</think>").Last().Trim();

Console.WriteLine(cleanedOutput2);
response2.Usage.OutputAsInformation(rawOutput2);
Utils.Separator();

// --- APPROACH 3: Instructional Control (Minimal Effort) ---
// Simplified: Pass instructions directly as the first parameter
ChatClientAgent agentMinimalEffort = GetCerebrasClient(modelId).CreateCerebrasAgent(
    instructions: "You are a direct assistant. Answer immediately without long internal thinking."
);

Console.WriteLine("--- Approach 3: Minimal Reasoning (Instructional Control) ---");
AgentRunResponse response3 = await agentMinimalEffort.RunAsync(question);

string rawOutput3 = response3.ToString();
string cleanedOutput3 = rawOutput3;
if (cleanedOutput3.Contains("</think>")) cleanedOutput3 = cleanedOutput3.Split("</think>").Last().Trim();

Console.WriteLine(cleanedOutput3);
response3.Usage.OutputAsInformation(rawOutput3);