#pragma warning disable MEAI001
using System.ClientModel;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
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

IChatClient routerClient = openAIClient.GetChatClient("qwen-3-32b").AsIChatClient();
IChatClient expertClient = openAIClient.GetChatClient("llama-3.3-70b").AsIChatClient();
IChatClient cheapClient = openAIClient.GetChatClient("llama3.1-8b").AsIChatClient();

ChatClientAgent intentAgent = routerClient.CreateCerebrasAgent(
    name: "IntentAgent",
    instructions: "Classify intent. Possible values: MusicQuestion, MovieQuestion, Other. Respond with JSON: {\"intent\": \"Value\"}"
);

Console.Write("> ");
string question = Console.ReadLine()!;

try
{
    // Using the new cleaned extension method
    var intentResult = await intentAgent.RunCerebrasAsync<IntentResult>(question);

    switch (intentResult.Intent)
    {
        case Intent.MusicQuestion:
            Utils.WriteLineGreen("Routing to Music Expert...");
            ChatClientAgent musicNerd = expertClient.CreateCerebrasAgent(
                name: "MusicNerd",
                instructions: "You are a Music Expert. Answer in max 200 chars."
            );
            ShowResponse(await musicNerd.RunAsync(question));
            break;

        case Intent.MovieQuestion:
            Utils.WriteLineGreen("Routing to Movie Expert...");
            ChatClientAgent movieNerd = expertClient.CreateCerebrasAgent(
                name: "MovieNerd",
                instructions: "You are a Movie Expert. Answer in max 200 chars."
            );
            ShowResponse(await movieNerd.RunAsync(question));
            break;

        case Intent.Other:
            Utils.WriteLineGreen("General Question...");
            ChatClientAgent generalAgent = cheapClient.CreateCerebrasAgent(
                name: "GeneralAssistant",
                instructions: "You are a helpful assistant."
            );
            ShowResponse(await generalAgent.RunAsync(question));
            break;
    }
}
catch (Exception ex)
{
    Utils.WriteLineRed($"Error: {ex.Message}");
}

void ShowResponse(AgentRunResponse agentRunResponse)
{
    string rawOutput = agentRunResponse.ToString();
    string cleanedOutput = rawOutput;

    if (cleanedOutput.Contains("</think>"))
        cleanedOutput = cleanedOutput.Split("</think>").Last().Trim();

    Console.WriteLine($"[AGENT]: {cleanedOutput}");
    agentRunResponse.Usage.OutputAsInformation(rawOutput);
}

public class IntentResult
{
    [JsonPropertyName("intent")]
    public required Intent Intent { get; set; }
}

public enum Intent { MusicQuestion, MovieQuestion, Other }