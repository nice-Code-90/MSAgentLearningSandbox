#pragma warning disable MEAI001
using OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using System.Reflection;
using System.Text;
using ToolCalling.Advanced.Tools;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.CerebrasApiKey;
string modelId = "llama-3.3-70b";


var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

IChatClient innerClient = openAIClient
    .GetChatClient(modelId)
    .AsIChatClient()
    .AsBuilder()
    .ConfigureOptions(options =>
    {
        options.AllowMultipleToolCalls = false;
    })
    .Build();

FileSystemTools target = new();
MethodInfo[] methods = typeof(FileSystemTools).GetMethods(BindingFlags.Public | BindingFlags.Instance);
List<AITool> listOfTools = methods
    .Select(x => AIFunctionFactory.Create(x, target))
    .Cast<AITool>()
    .ToList();

listOfTools.Add(new ApprovalRequiredAIFunction(AIFunctionFactory.Create(DangerousTools.SomethingDangerous)));

string actualRoot = target.GetRootFolder();

AIAgent agent = innerClient
    .CreateAIAgent(
        instructions: $"""
            You are a File Expert. 
            Your workspace is: {actualRoot}
            
            RULES:
            1. ALWAYS work inside this folder.
            2. If you are unsure where you are, call 'GetRootFolder' to confirm.
            3. Use full paths for every file operation.
            4. If the user asks "How many files", call 'GetFiles' using your workspace path.
            """,
        tools: listOfTools
    )
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

AgentThread thread = agent.GetNewThread();

Console.WriteLine("--- Advanced File Expert Agent Ready (Cerebras) ---");

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit") break;

    ChatMessage message = new(ChatRole.User, input);


    AgentRunResponse response = await agent.RunAsync(message, thread);


    List<UserInputRequestContent> userInputRequests = response.UserInputRequests.ToList();
    while (userInputRequests.Count > 0)
    {
        List<ChatMessage> userInputResponses = userInputRequests
            .OfType<FunctionApprovalRequestContent>()
            .Select(functionApprovalRequest =>
            {
                Console.WriteLine($"\n[APPROVAL REQUIRED] The agent wants to call: {functionApprovalRequest.FunctionCall.Name}");
                Console.Write("Type 'Y' to approve, any other key to deny: ");

                bool approved = Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false;

                return new ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(approved)]);
            })
            .ToList();


        response = await agent.RunAsync(userInputResponses, thread);
        userInputRequests = response.UserInputRequests.ToList();
    }


    Console.WriteLine(response);
    Utils.Separator();
}

async ValueTask<object?> FunctionCallMiddleware(
    AIAgent callingAgent,
    FunctionInvocationContext context,
    Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
    CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"[LOG] Tool Call: '{context.Function.Name}'");

    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" | Args: {string.Join(", ", context.Arguments.Select(x => $"{x.Key}={x.Value}"))}");
    }

    Utils.WriteLineDarkGray(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}