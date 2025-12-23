#pragma warning disable MEAI001
using OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using Shared.Extensions;

Secrets secrets = SecretManager.GetSecrets();

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri("https://api.cerebras.ai/v1"),
        NetworkTimeout = TimeSpan.FromMinutes(5)
    }
);

IChatClient client = openAIClient.GetChatClient(secrets.ModelId).AsIChatClient();

string question = "What is the Capital of France and how many people live there?";

ChatClientAgent agentDefault = client.CreateCerebrasAgent();

Console.WriteLine("--- Approach 1: Default Reasoning ---");
AgentRunResponse response1 = await agentDefault.RunAsync(question);

Console.WriteLine(response1.GetCleanContent());
response1.Usage.OutputAsInformation(response1.ToString());
Utils.Separator();

// --- APPROACH 2: Controlling Reasoning via Extension (Minimal Effort) ---
// We use the new reasoningEffort parameter in your library 

// Not supported in Cerebras SDK directly yet

// ChatClientAgent agentMinimalEffort = client.CreateCerebrasAgent(
//     reasoningEffortLevel: "minimal"
// );

// Console.WriteLine("--- Approach 2: Minimal Reasoning (via Effort Level) ---");
// AgentRunResponse response2 = await agentMinimalEffort.RunAsync(question);

// Console.WriteLine(response2.GetCleanContent());
// response2.Usage.OutputAsInformation(response2.ToString());
// Utils.Separator();

// --- APPROACH 3: Token Control (Legacy/Fallback method) ---
ChatClientAgent agentTokenLimit = client.CreateCerebrasAgent(
    instructions: "Answer immediately.",
    maxTokens: 500
);

Console.WriteLine("--- Approach 3: Limited Reasoning (via Max Tokens) ---");
AgentRunResponse response3 = await agentTokenLimit.RunAsync(question);

Console.WriteLine(response3.GetCleanContent());
response3.Usage.OutputAsInformation(response3.ToString());