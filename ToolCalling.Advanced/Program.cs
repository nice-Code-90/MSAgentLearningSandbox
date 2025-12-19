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

// 1. Titkok és Beállítások betöltése
Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.CerebrasApiKey;
string modelId = "llama-3.3-70b";

// 2. Cerebras Kliens inicializálása (OpenAI kompatibilis módon)
var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

// IChatClient absztrakció létrehozása
IChatClient innerClient = openAIClient.GetChatClient(modelId).AsIChatClient();

// 3. Toolok (eszközök) összegyűjtése Reflection segítségével
FileSystemTools target = new();
MethodInfo[] methods = typeof(FileSystemTools).GetMethods(BindingFlags.Public | BindingFlags.Instance);
List<AITool> listOfTools = methods
    .Select(x => AIFunctionFactory.Create(x, target))
    .Cast<AITool>()
    .ToList();

// Jóváhagyáshoz kötött "veszélyes" eszköz hozzáadása
listOfTools.Add(new ApprovalRequiredAIFunction(AIFunctionFactory.Create(DangerousTools.SomethingDangerous)));

string actualRoot = target.GetRootFolder();

// 4. AIAgent összeállítása Builder mintával
// Itt az innerClient-et használjuk az Azure kliens helyett
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

// 5. Interaktív hurok
while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit") break;

    ChatMessage message = new(ChatRole.User, input);

    // Első futtatás
    AgentRunResponse response = await agent.RunAsync(message, thread);

    // Human-in-the-loop: Jóváhagyási kérések kezelése
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

        // Válasz küldése az ügynöknek a jóváhagyás eredményével
        response = await agent.RunAsync(userInputResponses, thread);
        userInputRequests = response.UserInputRequests.ToList();
    }

    // Végső válasz kiírása
    Console.WriteLine(response);
    Utils.Separator();
}

// Middleware implementáció a tool hívások vizuális követéséhez
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