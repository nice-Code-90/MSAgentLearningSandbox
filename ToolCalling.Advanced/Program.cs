#pragma warning disable MEAI001
using OpenAI;
using OpenAI.Chat;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using System.Reflection;
using ToolCalling.Advanced.Tools;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Secrets secrets = SecretManager.GetSecrets();

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

FileSystemTools target = new();
MethodInfo[] methods = typeof(FileSystemTools).GetMethods(BindingFlags.Public | BindingFlags.Instance);
List<AITool> listOfTools = methods.Select(x => AIFunctionFactory.Create(x, target)).Cast<AITool>().ToList();
listOfTools.Add(new ApprovalRequiredAIFunction(AIFunctionFactory.Create(DangerousTools.SomethingDangerous)));

AIAgent agent = openAIClient
    .GetChatClient(secrets.ModelId)
    .AsAIAgent(
        instructions: $"You are a File Expert. Workspace: {target.GetRootFolder()}",
        tools: listOfTools
    )
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

AgentSession session = await agent.GetNewSessionAsync();
Console.WriteLine("--- Advanced File Expert Agent Ready (Cerebras) ---");

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit") break;

    ChatMessage message = new(ChatRole.User, input);
    AgentResponse response = await agent.RunAsync(message, session);

    List<UserInputRequestContent> userInputRequests = response.UserInputRequests.ToList();
    while (userInputRequests.Count > 0)
    {
        List<ChatMessage> userInputResponses = userInputRequests
            .OfType<FunctionApprovalRequestContent>()
            .Select(req =>
            {
                Console.WriteLine($"\n[APPROVAL] Agent wants to call: {req.FunctionCall.Name}");
                Console.Write("Approve (Y/N)? ");
                bool approved = Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false;
                return new ChatMessage(ChatRole.User, [req.CreateResponse(approved)]);
            }).ToList();

        response = await agent.RunAsync(userInputResponses, session);
        userInputRequests = response.UserInputRequests.ToList();
    }

    Console.WriteLine(response.GetCleanContent());

    Utils.Separator();
}

async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    Utils.WriteLineDarkGray($"[LOG] Tool Call: '{context.Function.Name}'");
    return await next(context, cancellationToken);
}