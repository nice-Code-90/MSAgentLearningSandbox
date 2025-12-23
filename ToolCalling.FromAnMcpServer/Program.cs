#pragma warning disable MEAI001
using OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using System.Text;
using ModelContextProtocol.Client;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using OpenAI.Chat;

Secrets secrets = SecretManager.GetSecrets();

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClient chatClient = openAIClient.GetChatClient(secrets.ModelId);

await using McpClient gitHubMcpClient = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
{
    TransportMode = HttpTransportMode.StreamableHttp,
    Endpoint = new Uri("https://api.githubcopilot.com/mcp/"),
    AdditionalHeaders = new Dictionary<string, string>
    {
        { "Authorization", $"Bearer {secrets.GitHubPatToken}" }
    }
}));

IList<McpClientTool> toolsInGitHubMcp = await gitHubMcpClient.ListToolsAsync();

AIAgent agent = chatClient.CreateCerebrasAgent(
   instructions: """
        You are a GitHub Expert.
        Your primary task is to provide direct answers based ONLY on the data returned by the tools.
        RULES:
        1. When a tool returns data (like a list of issues), summarize that specific data for the user.
        2. DO NOT provide general technical advice or code samples unless specifically asked.
        3. If the user asks for a count, just provide the number and a brief confirmation.
        4. Stay focused on the tool's output.
        """,
    tools: toolsInGitHubMcp.Cast<AITool>().ToList()
)
.AsBuilder()
.Use(FunctionCallMiddleware)
.Build();

AgentThread thread = agent.GetNewThread();

Console.WriteLine("--- GitHub MCP Expert Agent Ready (Cerebras) ---");

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit") break;

    ChatMessage message = new(ChatRole.User, input);
    AgentRunResponse response = await agent.RunAsync(message, thread);

    Console.WriteLine(response.GetCleanContent());

    Utils.Separator();
}

async ValueTask<object?> FunctionCallMiddleware(
    AIAgent callingAgent,
    FunctionInvocationContext context,
    Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
    CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");

    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))})");
    }

    Utils.WriteLineDarkGray(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}