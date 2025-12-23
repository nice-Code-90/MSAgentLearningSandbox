using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI.Chat;
using Microsoft.Extensions.AI;

namespace Shared.Extensions;

public static class CerebrasAgentExtensions
{
    public static ChatClientAgent CreateCerebrasAgent(
        this ChatClient chatClient,
        string? instructions = null,
        string? name = null,
        string? description = null,
        IList<AITool>? tools = null,
        int? maxTokens = null,
        string? reasoningEffortLevel = null,
        ILoggerFactory? loggerFactory = null,
        IServiceProvider? services = null)

    {

        IChatClient adaptedClient = chatClient.AsIChatClient();

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
                Temperature = 0.7f
            };
        }

        ChatClientAgentOptions clientAgentOptions = new()
        {
            Name = name,
            Description = description,
            ChatOptions = options
        };

        return new ChatClientAgent(adaptedClient, clientAgentOptions, loggerFactory, services);
    }


    public static string GetCleanContent(this AgentRunResponse response)
    {
        string content = response.ToString();

        if (content.Contains("<think>") || content.Contains("<|thinking|>"))
        {
            content = content.Split(new[] { "</tool_call>", "<|thinking|>" }, StringSplitOptions.None).Last().Trim();
        }
        if (content.StartsWith("Assistant: ", StringComparison.OrdinalIgnoreCase))
        {
            content = content.Substring("Assistant: ".Length).Trim();
        }
        return content;
    }


    public static async Task<T> RunCerebrasAsync<T>(this ChatClientAgent agent, string input)
    {
        var response = await agent.RunAsync(input);
        string content = response.GetCleanContent();

        int firstBrace = content.IndexOf('{');
        int lastBrace = content.LastIndexOf('}');

        if (firstBrace == -1 || lastBrace == -1)
        {
            var match = System.Text.RegularExpressions.Regex.Match(content, @"\{[^{}]*\}");
            if (match.Success)
            {
                content = match.Value;
            }
            else
            {
                throw new JsonException($"No valid JSON found in response. Cleaned content: '{content}'");
            }
        }
        else
        {
            content = content.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new JsonStringEnumConverter() }
        };

        try
        {
            return JsonSerializer.Deserialize<T>(content, options)
                ?? throw new JsonException("Deserialization returned null");
        }
        catch (Exception ex)
        {
            throw new JsonException($"Failed to deserialize response: '{content}'. Error: {ex.Message}", ex);
        }
    }
}