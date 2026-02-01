using OpenAI;
using OpenAI.Chat;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using ConversationThreads;

Secrets secrets = SecretManager.GetSecrets();

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

var agent = openAIClient
    .GetChatClient(secrets.ModelId)
    .AsAIAgent(instructions: "You are a Friendly AI Bot, answering questions");

AgentSession session;
const bool optionToResume = true;

if (optionToResume)
{
    session = await AgentThreadPersistence.ResumeChatIfRequestedAsync(agent);
}
else
{
    session = await agent.GetNewSessionAsync();
}

Console.WriteLine("--- Chat Started (Type 'exit' to stop) ---");

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit") break;

    Microsoft.Extensions.AI.ChatMessage message = new(Microsoft.Extensions.AI.ChatRole.User, input);

    await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(message, session))
    {
        Console.Write(update);
    }
    Console.WriteLine();

    Utils.Separator();

    if (optionToResume)
    {
        await AgentThreadPersistence.StoreThreadAsync(session);
    }
}