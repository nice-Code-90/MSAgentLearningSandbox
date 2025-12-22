#pragma warning disable MEAI001
using OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using Shared.Extensions;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

// 1. Setup Cerebras Secrets
Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.CerebrasApiKey;
string modelId = secrets.ModelId;

// 2. Initialize Cerebras Client
var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

IChatClient client = openAIClient.GetChatClient(modelId).AsIChatClient();

// 3. Create Agent using your extension method
ChatClientAgent agent = client.CreateCerebrasAgent(
    instructions: "You are a visual expert. Describe the content of the provided data carefully."
);

Scenario scenario = Scenario.Pdf; // Toggle this to test different inputs

AgentRunResponse response;
switch (scenario)
{
    case Scenario.Text:
        {
            response = await agent.RunAsync("What is the capital of France?");
            ShowResponse(response);
        }
        break;

    case Scenario.Image:
        {
            // NOTE: This will likely fail on Cerebras until they support Vision models.
            // But this is the standard way to do it in the framework.

            // Image via URI
            Console.WriteLine("--- Testing Image via URI ---");
            response = await agent.RunAsync(new ChatMessage(ChatRole.User,
            [
                new TextContent("What is in this image?"),
                new UriContent("https://upload.wikimedia.org/wikipedia/commons/7/70/A_game_of_Settlers_of_Catan.jpg", "image/jpeg")
            ]));
            ShowResponse(response);

            // Image via Base64 (Local File)
            Console.WriteLine("\n--- Testing Local Image File ---");
            string path = Path.Combine("SampleData", "image.jpg");
            if (File.Exists(path))
            {
                ReadOnlyMemory<byte> data = File.ReadAllBytes(path).AsMemory();
                response = await agent.RunAsync(new ChatMessage(ChatRole.User,
                [
                    new TextContent("What is in this local image?"),
                    new DataContent(data, "image/jpeg")
                ]));
                ShowResponse(response);
            }
        }
        break;

    case Scenario.Pdf:
        {
            // NOTE: PDF support is model-specific.
            Console.WriteLine("--- Testing PDF Input ---");
            string path = Path.Combine("SampleData", "catan_rules.pdf");
            if (File.Exists(path))
            {
                ReadOnlyMemory<byte> data = File.ReadAllBytes(path).AsMemory();
                response = await agent.RunAsync(new ChatMessage(ChatRole.User,
                [
                    new TextContent("What are the winning conditions in this PDF?"),
                    new DataContent(data, "application/pdf"),
                ]));
                ShowResponse(response);
            }
        }
        break;
}

void ShowResponse(AgentRunResponse agentRunResponse)
{
    string rawOutput = agentRunResponse.ToString();
    string cleanedOutput = rawOutput;

    if (cleanedOutput.Contains("</think>"))
        cleanedOutput = cleanedOutput.Split("</think>").Last().Trim();

    Console.WriteLine($"[AGENT]: {cleanedOutput}");

    // Using your enhanced token estimation logic
    agentRunResponse.Usage.OutputAsInformation(rawOutput);
}

enum Scenario { Text, Pdf, Image }