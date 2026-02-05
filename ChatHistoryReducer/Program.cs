using OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using Shared.Extensions;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

#pragma warning disable MEAI001

Console.Clear();
Secrets secrets = SecretManager.GetSecrets();

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

IChatClient chatClient = openAIClient.GetChatClient(secrets.ModelId).AsIChatClient();

IChatReducer messageCountReducer = new MessageCountingChatReducer(targetCount: 4);

IChatReducer summaryReducer = new SummarizingChatReducer(chatClient, targetCount: 1, threshold: 4);

ChatClientAgent agent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
        Name = "CerebrasAgent",
        ChatOptions = new ChatOptions
        {
            Instructions = "You are a helpful assistant that remembers details, but efficiently."
        },
        ChatHistoryProviderFactory = (context, token) =>
            ValueTask.FromResult<ChatHistoryProvider>(
                new InMemoryChatHistoryProvider(summaryReducer, context.SerializedState, context.JsonSerializerOptions))
    }
);

AgentSession session = await agent.GetNewSessionAsync();

Utils.WriteLineGreen($"***Start Cerebras Agent ({secrets.ModelId}) ***");
Utils.WriteLineYellow("Try it: Say your name, then ask lots of questions to see the reduction!");

while (true)
{
    Console.Write("\n> ");
    string input = Console.ReadLine() ?? string.Empty;
    if (input.ToLower() == "exit") break;

    AgentResponse response = await agent.RunAsync(input, session);

    Console.WriteLine($"\nAssistant: {response.GetCleanContent()}");

    response.Usage.OutputAsInformation(response.ToString());

    IList<ChatMessage> messagesInSession = session.GetService<IList<ChatMessage>>()!;
    Utils.WriteLineDarkGray($"- Number of messages in memory: {messagesInSession.Count}");

    foreach (ChatMessage message in messagesInSession)
    {
        string preview = message.Text?.Length > 50 ? message.Text[..50] + "..." : message.Text!;
        Utils.WriteLineDarkGray($"  [{message.Role}]: {preview}");
    }

    Utils.Separator();
}