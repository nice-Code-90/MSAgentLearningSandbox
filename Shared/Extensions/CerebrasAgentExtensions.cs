using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI.Chat;

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
        string? reasoningEffortLevel = null,
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
        if (!string.IsNullOrWhiteSpace(reasoningEffortLevel))
        {
            options.RawRepresentationFactory = _ => new ChatCompletionOptions
            {
#pragma warning disable OPENAI001
                ReasoningEffortLevel = reasoningEffortLevel,
#pragma warning restore OPENAI001
            };
        }

        ChatClientAgentOptions clientAgentOptions = new()
        {
            Name = name,
            Description = description,
            ChatOptions = options
        };

        return new ChatClientAgent(chatClient, clientAgentOptions, loggerFactory, services);
    }

    public static string GetCleanContent(this AgentRunResponse response)
    {
        string content = response.ToString();
        return content.Contains("</think>") ? content.Split("</think>").Last().Trim() : content;
    }

    public static async Task<T> RunCerebrasAsync<T>(this ChatClientAgent agent, string input)
    {
        var response = await agent.RunAsync(input);
        string content = response.GetCleanContent();
        int firstBrace = content.IndexOf('{');
        int lastBrace = content.LastIndexOf('}');
        if (firstBrace == -1 || lastBrace == -1)
        {
            throw new JsonException($"No valid JSON found in response: {content}");
        }

        string jsonContent = content.Substring(firstBrace, lastBrace - firstBrace + 1);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Deserialize<T>(jsonContent, options)
           ?? throw new JsonException("Failed to deserialize agent response.");
    }
}