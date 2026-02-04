#pragma warning disable MEAI001
using OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using StructuredOutput.Models;
using ChatResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat;

Secrets secrets = SecretManager.GetSecrets();
string modelId = secrets.ModelId;

var openAIClient = new OpenAIClient(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

IChatClient client = openAIClient
    .GetChatClient(modelId)
    .AsIChatClient()
    .AsBuilder()
    .ConfigureOptions(options => options.AllowMultipleToolCalls = false)
    .Build();

string question = "What are the top 10 Movies according to IMDB?";

// --- APPROACH 1: Standard AIAgent (Raw Text) ---
// This response is just a string and isn't guaranteed to follow any specific structure.
// AIAgent agent1 = client.CreateAIAgent(instructions: "You are an expert in IMDB Lists");

// AgentRunResponse response1 = await agent1.RunAsync(question);
// Console.WriteLine("--- Approach 1: Raw Text Output ---");
// Console.WriteLine(response1);

// Utils.Separator();

// --- APPROACH 2: ChatClientAgent with Generic RunAsync (Automatic) ---
// This is the cleanest way. By using ChatClientAgent, we can call the generic RunAsync<T>.
// The framework handles the JSON schema generation and deserialization automatically.
ChatClientAgent agent2 = client.AsAIAgent(instructions: "You are an expert in IMDB Lists");

Console.WriteLine("--- Approach 2: Structured Output (Generic) ---");
AgentResponse<MovieResult> response2 = await agent2.RunAsync<MovieResult>(question);


DisplayMovies(response2.Result);

Utils.Separator();

// --- APPROACH 3: Manual JSON Schema Configuration ---
// Sometimes you need specific serializer options for complex types.
JsonSerializerOptions jsonSerializerOptions = new()
{
    PropertyNameCaseInsensitive = true,
    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    Converters = { new JsonStringEnumConverter() }
};

AIAgent agent3 = client.AsAIAgent(instructions: "You are an expert in IMDB Lists");


ChatResponseFormatJson chatResponseFormatJson = ChatResponseFormat.ForJsonSchema<MovieResult>(jsonSerializerOptions);

Console.WriteLine("--- Approach 3: Explicit JSON Schema & Manual Deserialization ---");
AgentResponse response3 = await agent3.RunAsync(question, options: new ChatClientAgentRunOptions()
{
    ChatOptions = new ChatOptions
    {
        ResponseFormat = chatResponseFormatJson
    }
});

MovieResult movieResult3 = response3.Deserialize<MovieResult>(jsonSerializerOptions);
DisplayMovies(movieResult3);

void DisplayMovies(MovieResult movieResult)
{
    if (movieResult == null) return;

    Console.WriteLine(movieResult.MessageBack);
    Console.WriteLine(new string('-', 30));

    int counter = 1;
    foreach (Movie movie in movieResult.Top10Movies)
    {
        Console.WriteLine($"{counter}: {movie.Title} ({movie.YearOfRelease})");
        Console.WriteLine($"   Genre: {movie.Genre} | Director: {movie.Director} | IMDB Score: {movie.ImdbScore}");
        counter++;
    }
}