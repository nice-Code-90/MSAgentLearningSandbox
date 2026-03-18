using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ChatResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat;

namespace AgentFrameworkToolkit.EasierAgents;

public class Before
{
    public static async Task RunAsync()
    {
        Secrets secrets = SecretManager.GetSecrets();
        string apiKey = secrets.LLMApiKey;
        string modelId = secrets.ModelId;
        OpenAIClient cerebrasClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

        AIAgent agent = cerebrasClient
            .GetChatClient(modelId)
            .AsAIAgent(
                options: new ChatClientAgentOptions
                {
                    ChatOptions = new ChatOptions
                    {
                        RawRepresentationFactory = _ => new ChatCompletionOptions
                        {
#pragma warning disable OPENAI001

#pragma warning restore OPENAI001
                        },
                        Tools = [AIFunctionFactory.Create(WeatherTool.GetWeather)]
                    }
                })
            .AsBuilder()
            .Use(FunctionCallMiddleware)
            .Build();

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters = { new JsonStringEnumConverter() }
        };

        AgentResponse response = await agent.RunAsync("What is the Weather like in Paris",
            options: new ChatClientAgentRunOptions()
            {
                ChatOptions = new ChatOptions
                {
                    ResponseFormat = ChatResponseFormat.ForJsonSchema<WeatherReport>(jsonSerializerOptions)
                }
            }
        );
        WeatherReport weatherReport = JsonSerializer.Deserialize<WeatherReport>(response.Text, jsonSerializerOptions)!;
        Console.WriteLine("City: " + weatherReport.City);
        Console.WriteLine("Condition: " + weatherReport.Condition);
        Console.WriteLine("Degrees: " + weatherReport.Degrees);
        Console.WriteLine("Fahrenheit: " + weatherReport.Fahrenheit);
        response.Usage.OutputAsInformation();
    }

    private static async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
    {
        StringBuilder functionCallDetails = new();
        functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
        if (context.Arguments.Count > 0)
        {
            functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
        }

        Utils.WriteLineDarkGray(functionCallDetails.ToString());

        return await next(context, cancellationToken);
    }
}