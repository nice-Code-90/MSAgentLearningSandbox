#pragma warning disable MEAI001
using OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using Shared.Extensions;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using OpenAI.Chat;

Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.CerebrasApiKey;
string modelId = secrets.ModelId;

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClient client = openAIClient.GetChatClient(modelId);

ChatClientAgent agent = client.CreateCerebrasAgent(
    instructions: "You are a visual expert. Describe the content of the provided data carefully."
);

Scenario scenario = Scenario.Pdf;

AgentResponse response;
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

void ShowResponse(AgentResponse agentRunResponse)
{
    string rawOutput = agentRunResponse.ToString();
    string cleanedOutput = rawOutput;

    if (cleanedOutput.Contains("</think>"))
        cleanedOutput = cleanedOutput.Split("</think>").Last().Trim();

    Console.WriteLine($"[AGENT]: {cleanedOutput}");

    agentRunResponse.Usage.OutputAsInformation(rawOutput);
}

enum Scenario { Text, Pdf, Image }