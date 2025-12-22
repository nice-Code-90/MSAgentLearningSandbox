using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Shared.Extensions;

public static class CerebrasAgentExtensions
{

    public static ChatClientAgent CreateCerebrasAgent(
        this IChatClient chatClient,
        string? instructions = null,
        string? name = null,
        string? description = null,
        IList<AITool>? tools = null,
        int? maxTokens = null,
        ILoggerFactory? loggerFactory = null,
        IServiceProvider? services = null)
    {
        ChatOptions options = new()
        {
            Instructions = instructions,
            MaxOutputTokens = maxTokens
        };

        if (tools?.Count > 0)
        {
            options.Tools = tools;
        }

        ChatClientAgentOptions clientAgentOptions = new()
        {
            Name = name,
            Description = description,
            ChatOptions = options
        };

        return new ChatClientAgent(chatClient, clientAgentOptions, loggerFactory, services);
    }
}