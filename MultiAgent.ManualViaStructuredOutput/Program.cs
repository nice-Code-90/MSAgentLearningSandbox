using System.ComponentModel;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using Shared;
using Shared.Extensions;
using System.ClientModel;

Secrets secrets = SecretManager.GetSecrets();

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);


ChatClient intentChatClient = openAIClient.GetChatClient("llama3.1-8b");
ChatClient answerChatClient = openAIClient.GetChatClient("llama-3.3-70b");

Console.Write("> ");
string question = Console.ReadLine()!;

ChatClientAgent intentAgent = intentChatClient.CreateCerebrasAgent(
    name: "IntentAgent",
    instructions: "Determine what type of question was asked. Never answer yourself. Respond ONLY with a JSON object in this exact format: { \"Intent\": \"MusicQuestion\" } or { \"Intent\": \"MovieQuestion\" } or { \"Intent\": \"Other\" }. Do not add any other text or explanation."
);

IntentResult intentResult;
try
{
    intentResult = await intentAgent.RunCerebrasAsync<IntentResult>(question);
}
catch (Exception ex)
{
    Console.WriteLine($"Error determining intent: {ex.Message}. Defaulting to 'Other'.");
    intentResult = new IntentResult { Intent = Intent.Other };
}

switch (intentResult.Intent)
{
    case Intent.MusicQuestion:
        Utils.WriteLineGreen("Music Question");
        ChatClientAgent musicNerdAgent = answerChatClient.CreateCerebrasAgent(
            name: "MusicNerd",
            instructions: "You are a Music Nerd answering questions. Keep your answer under 200 characters. Be concise and accurate."
        );
        AgentRunResponse responseFromMusicNerd = await musicNerdAgent.RunAsync(question);
        Console.WriteLine(responseFromMusicNerd.GetCleanContent());
        break;

    case Intent.MovieQuestion:
        Utils.WriteLineGreen("Movie Question");
        ChatClientAgent movieNerdAgent = answerChatClient.CreateCerebrasAgent(
            name: "MovieNerd",
            instructions: "You are a Movie Nerd answering questions. Keep your answer under 200 characters. Be concise and accurate."
        );
        AgentRunResponse responseFromMovieNerd = await movieNerdAgent.RunAsync(question);
        Console.WriteLine(responseFromMovieNerd.GetCleanContent());
        break;

    case Intent.Other:
        Utils.WriteLineGreen("Other Question");
        ChatClientAgent generalAgent = answerChatClient.CreateCerebrasAgent(
            name: "GeneralAssistant",
            instructions: "Answer the user's question concisely and accurately. Keep your answer under 200 characters."
        );
        AgentRunResponse otherResponse = await generalAgent.RunAsync(question);
        Console.WriteLine(otherResponse.GetCleanContent());
        break;

    default:
        Console.WriteLine($"Unknown intent: {intentResult.Intent}. Treating as 'Other'.");
        ChatClientAgent fallbackAgent = answerChatClient.CreateCerebrasAgent(
            instructions: "Answer the user's question concisely and accurately."
        );
        AgentRunResponse fallbackResponse = await fallbackAgent.RunAsync(question);
        Console.WriteLine(fallbackResponse.GetCleanContent());
        break;
}

Console.ReadKey();

public class IntentResult
{
    [Description("What type of question is this?")]
    public required Intent Intent { get; set; }
}

public enum Intent
{
    MusicQuestion,
    MovieQuestion,
    Other
}