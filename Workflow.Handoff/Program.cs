using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Secrets secrets = SecretManager.GetSecrets();

OpenAIClient cerebrasClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClient routerClient = cerebrasClient.GetChatClient("qwen-3-32b");
ChatClient expertClient = cerebrasClient.GetChatClient("llama-3.3-70b");

ChatClientAgent intentAgent = routerClient.CreateCerebrasAgent(
    name: "IntentAgent",
    instructions: "Determine what type of question was asked (Movie or Music). Never answer the question yourself, always hand it over to the appropriate expert."
);

ChatClientAgent movieNerd = expertClient.CreateCerebrasAgent(
    name: "MovieNerd",
    instructions: "You are a Movie Nerd expert. Answer questions about films, directors, and cinema history."
);

ChatClientAgent musicNerd = expertClient.CreateCerebrasAgent(
    name: "MusicNerd",
    instructions: "You are a Music Nerd expert. Answer questions about bands, albums, and music theory."
);



while (true)
{
    List<ChatMessage> messages = [];
    Console.Write("\n> ");
    string input = Console.ReadLine()!;
    if (string.IsNullOrWhiteSpace(input)) break;

    Workflow workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(intentAgent)
    .WithHandoffs(intentAgent, [movieNerd, musicNerd])
    .WithHandoffs([movieNerd, musicNerd], intentAgent)
    .Build();

    messages.Add(new(ChatRole.User, input));


    var resultMessages = await RunWorkflowAsync(workflow, messages);
    messages.AddRange(resultMessages);
}

static async Task<List<ChatMessage>> RunWorkflowAsync(Workflow workflow, List<ChatMessage> messages)
{
    string? lastExecutorId = null;

    StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

    await foreach (WorkflowEvent @event in run.WatchStreamAsync())
    {
        switch (@event)
        {
            case AgentRunUpdateEvent e:
                {
                    if (e.ExecutorId != lastExecutorId)
                    {
                        lastExecutorId = e.ExecutorId;
                        Console.WriteLine();
                        Utils.WriteLineGreen($"[{e.Update.AuthorName ?? e.ExecutorId}]");
                    }

                    if (!string.IsNullOrEmpty(e.Update.Text))
                    {
                        Console.Write(e.Update.Text);
                    }

                    if (e.Update.Contents.OfType<FunctionCallContent>().FirstOrDefault() is FunctionCallContent call)
                    {
                        Console.WriteLine();
                        Utils.WriteLineDarkGray($"[Action: {call.Name}]");
                    }
                    break;
                }
            case WorkflowOutputEvent output:
                Utils.Separator();
                return output.As<List<ChatMessage>>()!;

            case ExecutorFailedEvent failedEvent:
                if (failedEvent.Data is Exception ex)
                {
                    Utils.WriteLineRed($"Error in agent {failedEvent.ExecutorId}: {ex.Message}");
                }
                break;
        }
    }
    return [];
}