using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;

Secrets secrets = SecretManager.GetSecrets();
string apiKey = secrets.LLMApiKey;
string modelId = secrets.ModelId;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

// Register Services needed to run DevUI
builder.Services.AddChatClient(openAIClient.GetChatClient(modelId).AsIChatClient());
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

// Register "dummy" Agent
builder.AddAIAgent("Comic Book Guy", "You are comic-book guy from The Simpsons")
    .WithAITool(AIFunctionFactory.Create(GetWeather));

//Build a "normal" Agent
string realAgentName = "Real Agent";
AIAgent myAgent = openAIClient
    .GetChatClient(modelId)
    .AsAIAgent(name: realAgentName, instructions: "Speak like a pirate", tools: [AIFunctionFactory.Create(GetWeather)]);

builder.AddAIAgent(realAgentName, (serviceProvider, key) => myAgent);

// Register sample workflows
IHostedAgentBuilder frenchTranslator = builder.AddAIAgent("french-translator", "Translate any text you get into French");
IHostedAgentBuilder germanTranslator = builder.AddAIAgent("german-translator", "Translate any text you get into German");

builder.AddWorkflow("translation-workflow-sequential", (sp, key) =>
{
    IEnumerable<AIAgent> agentsForWorkflow = new List<IHostedAgentBuilder>() { frenchTranslator, germanTranslator }.Select(ab => sp.GetRequiredKeyedService<AIAgent>(ab.Name));
    return AgentWorkflowBuilder.BuildSequential(workflowName: key, agents: agentsForWorkflow);
}).AddAsAIAgent();

builder.AddWorkflow("translation-workflow-concurrent", (sp, key) =>
{
    IEnumerable<AIAgent> agentsForWorkflow = new List<IHostedAgentBuilder>() { frenchTranslator, germanTranslator }.Select(ab => sp.GetRequiredKeyedService<AIAgent>(ab.Name));
    return AgentWorkflowBuilder.BuildConcurrent(workflowName: key, agents: agentsForWorkflow);
}).AddAsAIAgent();


WebApplication app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    //Needed for DevUI to function 
    app.MapOpenAIResponses();
    app.MapOpenAIConversations();
    app.MapDevUI();
}

app.Run();

static string GetWeather(string city)
{
    return "It is sunny and 19 degrees";
}