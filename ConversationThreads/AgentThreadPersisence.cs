using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace ConversationThreads;

public static class AgentThreadPersistence
{
    private static string ConversationPath => Path.Combine(Path.GetTempPath(), "conversation.json");

    public static async Task<AgentSession> ResumeChatIfRequestedAsync(ChatClientAgent agent)
    {
        if (File.Exists(ConversationPath))
        {
            Console.Write("Restore previous conversation? (Y/N): ");
            ConsoleKeyInfo key = Console.ReadKey();

            if (key.Key == ConsoleKey.Y)
            {
                JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(await File.ReadAllTextAsync(ConversationPath));
                AgentSession resumedSession =await agent.DeserializeSessionAsync(jsonElement);
                await RestoreConsole(resumedSession);
                return resumedSession;
            }
        }

        return await agent.GetNewSessionAsync();   
    }

private static async Task RestoreConsole(AgentSession resumedSession)
    {
        var messages = resumedSession.GetService<IList<ChatMessage>>();

        if (messages != null)
        {
            foreach (ChatMessage message in messages)
            {
                if (message.Role == ChatRole.User)
                {
                    Console.WriteLine($"> {message.Text}");
                }
                else if (message.Role == ChatRole.Assistant)
                {
                    Console.WriteLine($"{message.Text}");
                    Console.WriteLine();
                    Console.WriteLine(new string('*', 50));
                    Console.WriteLine();
                }
            }
        }
    }

    public static async Task StoreThreadAsync(AgentSession session)
    {
        JsonElement serializedSession = session.Serialize();
        await File.WriteAllTextAsync(ConversationPath, JsonSerializer.Serialize(serializedSession));
    }
}