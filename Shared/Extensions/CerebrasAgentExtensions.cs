using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public static async Task<T> RunCerebrasAsync<T>(this ChatClientAgent agent, string input)
    {
        var response = await agent.RunAsync(input);
        string content = response.ToString();

        if (content.Contains("</think>"))
        {
            content = content.Split("</think>").Last().Trim();
        }

        content = content.Replace("```json", "").Replace("```", "").Trim();


        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new JsonStringEnumConverter() }
        };

        var result = JsonSerializer.Deserialize<T>(content, options);

        return result ?? throw new JsonException("Failed to deserialize agent response.");
    }
}