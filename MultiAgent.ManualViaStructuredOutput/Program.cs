#pragma warning disable MEAI001
using System.ClientModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;
using OpenAI;

Secrets secrets = SecretManager.GetSecrets();
var openAIClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

var intentSchema = ChatResponseFormat.ForJsonSchema<IntentResult>();

IChatClient classificationClient = openAIClient
    .GetChatClient(secrets.ModelId)
    .AsIChatClient()
    .AsBuilder()
    .ConfigureOptions(options =>
    {
        options.ResponseFormat = intentSchema;
        options.MaxOutputTokens = 100;
    })
    .Build();

IChatClient expertClient = openAIClient.GetChatClient("llama-3.3-70b").AsIChatClient();

ChatClientAgent intentAgent = classificationClient.CreateCerebrasAgent(
    name: "IntentAgent",
    instructions: """
        Your ONLY task is to classify user input into: MusicQuestion, MovieQuestion, or Other.
        DO NOT use internal reasoning (<think> tags).
        DO NOT provide any text other than the JSON object.
        JSON format: {"intent": "Value"}
        """
);

ChatClientAgent generalAgent = expertClient.CreateCerebrasAgent(
    name: "GeneralAssistant",
    instructions: "You are a helpful general assistant."
);

Console.Write("> ");
string? question = Console.ReadLine();
if (string.IsNullOrWhiteSpace(question)) return;

try
{
    var intentResult = await intentAgent.RunCerebrasAsync<IntentResult>(question);

    switch (intentResult.Intent)
    {
        case Intent.MusicQuestion:
            Utils.WriteLineGreen("[ROUTING]: Music Expert");
            ChatClientAgent musicNerd = expertClient.CreateCerebrasAgent(
                name: "MusicNerd",
                instructions: "You are a Music Expert. Answer in max 200 chars."
            );
            await RunAndShow(musicNerd, question);
            break;

        case Intent.MovieQuestion:
            Utils.WriteLineGreen("[ROUTING]: Movie Expert");
            ChatClientAgent movieNerd = expertClient.CreateCerebrasAgent(
                name: "MovieNerd",
                instructions: "You are a Movie Nerd. Answer in max 200 chars."
            );
            await RunAndShow(movieNerd, question);
            break;

        case Intent.Other:
            Utils.WriteLineGreen("[ROUTING]: General Assistant");
            await RunAndShow(generalAgent, question);
            break;
    }
}
catch (Exception ex)
{
    Utils.WriteLineRed($"Routing Failed. Reason: {ex.Message}");
    Utils.WriteLineYellow("Falling back to General Assistant...");
    await RunAndShow(generalAgent, question);
}

async Task RunAndShow(ChatClientAgent agent, string input)
{
    AgentRunResponse response = await agent.RunAsync(input);
    Console.WriteLine($"\n[{agent.Name}]: {response.GetCleanContent()}");
    Utils.Separator();
}

public class IntentResult
{
    [Description("Classified intent: MusicQuestion, MovieQuestion, or Other")]
    [JsonPropertyName("intent")]
    public required Intent Intent { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Intent { MusicQuestion, MovieQuestion, Other }