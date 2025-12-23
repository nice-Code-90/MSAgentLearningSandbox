using OpenAI;
using OpenAI.Chat;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using ToolCalling.Basics;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Secrets secrets = SecretManager.GetSecrets();

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClientAgent agent = openAIClient
    .GetChatClient(secrets.ModelId)
    .CreateAIAgent(
        instructions: "You are a Time Expert. Use the provided tools to answer questions about time and dates.",
        tools:
        [
            AIFunctionFactory.Create(Tools.CurrentDataAndTime, "current_date_and_time"),
            AIFunctionFactory.Create(Tools.CurrentTimezone, "current_timezone")
        ]
    );

AgentThread thread = agent.GetNewThread();

Console.WriteLine("--- Time Expert Agent Ready ---");
Console.WriteLine("Try asking: 'What time is it?' or 'What is the current date?'");

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit") break;

    ChatMessage message = new(ChatRole.User, input);

    AgentRunResponse response = await agent.RunAsync(message, thread);

    Console.WriteLine(response);
    Utils.Separator();
}