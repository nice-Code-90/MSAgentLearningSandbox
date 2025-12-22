#pragma warning disable MEAI001
using System.ClientModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using Shared.Extensions;

// 1. Setup Cerebras Client
Secrets secrets = SecretManager.GetSecrets();
var openAIClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri("https://api.cerebras.ai/v1"),
        NetworkTimeout = TimeSpan.FromMinutes(5)
    }
);

// Tiered Model Strategy: Fast model for routing, Powerful model for reasoning
IChatClient clientMini = openAIClient.GetChatClient("llama3.1-8b").AsIChatClient();
IChatClient clientMain = openAIClient.GetChatClient("qwen-3-32b").AsIChatClient();

// 2. Setup Intent Agent with strict instructions to ensure valid JSON output
ChatClientAgent intentAgent = clientMini.CreateCerebrasAgent(
    name: "IntentAgent",
    instructions: "Determine the intent of the user question. Respond ONLY with a valid JSON object. No markdown, no prose, no conversational filler."
);

Console.Write("> ");
string question = Console.ReadLine()!;

// 3. Determine Intent using Structured Output
// We use a try-catch to handle potential formatting errors from smaller models
try
{
    AgentRunResponse<IntentResult> initialResponse = await intentAgent.RunAsync<IntentResult>(question);
    IntentResult intentResult = initialResponse.Result;

    // 4. Dispatch based on Intent to Specialized Agents
    ChatClientAgent? selectedAgent = null;

    switch (intentResult.Intent)
    {
        case Intent.MusicQuestion:
            Utils.WriteLineGreen("Routing to Music Nerd (Qwen)...");
            selectedAgent = clientMain.CreateCerebrasAgent(
                name: "MusicNerd",
                instructions: "You are a Music Nerd. Answer in max 200 characters."
            );
            break;

        case Intent.MovieQuestion:
            Utils.WriteLineGreen("Routing to Movie Nerd (Qwen)...");
            selectedAgent = clientMain.CreateCerebrasAgent(
                name: "MovieNerd",
                instructions: "You are a Movie Nerd. Answer in max 200 characters."
            );
            break;

        case Intent.Other:
            Utils.WriteLineGreen("Routing to General Assistant (Qwen)...");
            selectedAgent = clientMain.CreateCerebrasAgent(
                name: "GeneralAssistant",
                instructions: "You are a helpful assistant."
            );
            break;
    }

    if (selectedAgent != null)
    {
        AgentRunResponse finalResponse = await selectedAgent.RunAsync(question);
        ShowResponse(finalResponse);
    }
}
catch (Exception ex)
{
    Utils.WriteLineRed($"Routing Error: {ex.Message}");
    Console.WriteLine("Falling back to Main Model for intent detection...");

    // Fallback: Use Qwen if Llama fails to produce valid JSON
    ChatClientAgent fallbackAgent = clientMain.CreateCerebrasAgent(instructions: "Categorize the user intent.");
    AgentRunResponse fallbackResponse = await fallbackAgent.RunAsync(question);
    ShowResponse(fallbackResponse);
}

void ShowResponse(AgentRunResponse agentRunResponse)
{
    string rawOutput = agentRunResponse.ToString();
    string cleanedOutput = rawOutput;

    // Filter out internal reasoning monologues
    if (cleanedOutput.Contains("</think>"))
        cleanedOutput = cleanedOutput.Split("</think>").Last().Trim();

    Console.WriteLine($"[AGENT]: {cleanedOutput}");

    // Execute manual reasoning token estimation
    agentRunResponse.Usage.OutputAsInformation(rawOutput);
}

public class IntentResult
{
    [Description("What type of question is this?")]
    [JsonPropertyName("intent")] // Fixes Llama case-sensitivity issues
    public required Intent Intent { get; set; }
}

public enum Intent
{
    MusicQuestion,
    MovieQuestion,
    Other
}