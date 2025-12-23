using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MultiAgent.AgentAsTool;
using OpenAI;
using OpenAI.Chat;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using System.Text;

Secrets secrets = SecretManager.GetSecrets();

OpenAIClient cerebrasClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClient chatClient = cerebrasClient.GetChatClient(secrets.ModelId);

var stringAgent = chatClient
    .CreateCerebrasAgent(
        name: "StringAgent",
        instructions: "You are string manipulator",
        tools:
        [
            AIFunctionFactory.Create(StringTools.Reverse),
            AIFunctionFactory.Create(StringTools.Uppercase),
            AIFunctionFactory.Create(StringTools.Lowercase)
        ])
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

var numberAgent = chatClient
    .CreateCerebrasAgent(
        name: "NumberAgent",
        instructions: "You are a number expert",
        tools:
        [
            AIFunctionFactory.Create(NumberTools.RandomNumber),
            AIFunctionFactory.Create(NumberTools.AnswerToEverythingNumber)
        ])
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

Utils.WriteLineGreen("DELEGATE AGENT (Cerebras)");

var delegationAgent = chatClient
    .CreateCerebrasAgent(
        name: "DelegateAgent",
        instructions: "Are a Delegator of String and Number Tasks. Never does such work yourself",
        tools:
        [
            stringAgent.AsAIFunction(new AIFunctionFactoryOptions
            {
                Name = "StringAgentAsTool"
            }),
            numberAgent.AsAIFunction(new AIFunctionFactoryOptions
            {
                Name = "NumberAgentAsTool"
            })
        ]
    )
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();


AgentRunResponse responseFromDelegate = await delegationAgent.RunAsync("Uppercase 'Hello World'");
Console.WriteLine(responseFromDelegate.GetCleanContent());
responseFromDelegate.Usage.OutputAsInformation();

Utils.Separator();

Utils.WriteLineGreen("JACK OF ALL TRADE AGENT (Cerebras)");

var jackOfAllTradesAgent = chatClient
    .CreateCerebrasAgent(
        name: "JackOfAllTradesAgent",
        instructions: "Are a Agent that can answer questions on strings and numbers",
        tools:
        [
            AIFunctionFactory.Create(StringTools.Reverse),
            AIFunctionFactory.Create(StringTools.Uppercase),
            AIFunctionFactory.Create(StringTools.Lowercase),
            AIFunctionFactory.Create(NumberTools.RandomNumber),
            AIFunctionFactory.Create(NumberTools.AnswerToEverythingNumber)
        ]
    )
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

AgentRunResponse responseFromJackOfAllTrade = await jackOfAllTradesAgent.RunAsync("Uppercase 'Hello World'");
Console.WriteLine(responseFromJackOfAllTrade.GetCleanContent());
responseFromJackOfAllTrade.Usage.OutputAsInformation();

async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"- Tool Call: '{context.Function.Name}' [Agent: {callingAgent.Name}]");
    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))})");
    }

    Utils.WriteLineDarkGray(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}