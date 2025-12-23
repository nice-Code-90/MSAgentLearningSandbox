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
    .CreateAIAgent(instructions: "You are a Friendly AI Bot, answering questions");

AgentThread thread;
const bool optionToResume = true;

if (optionToResume)
{
    thread = await AgentThreadPersistence.ResumeChatIfRequestedAsync(agent);
}
else
{
    thread = agent.GetNewThread();
}

Console.WriteLine("--- Chat Started (Type 'exit' to stop) ---");

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit") break;

    Microsoft.Extensions.AI.ChatMessage message = new(Microsoft.Extensions.AI.ChatRole.User, input);

    await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync(message, thread))
    {
        Console.Write(update);
    }
    Console.WriteLine();

    Utils.Separator();

    if (optionToResume)
    {
        await AgentThreadPersistence.StoreThreadAsync(thread);
    }
}