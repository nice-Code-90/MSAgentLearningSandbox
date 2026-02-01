using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI;
using OpenAI.Chat;
using Shared;
using Shared.AI;
using Shared.Extensions;
using System.ClientModel;
using System.Text.Json;
using UsingRAGInAgentFramework.Models;

string jsonWithMovies = await File.ReadAllTextAsync("made_up_movies.json");
Movie[] movieDataForRag = JsonSerializer.Deserialize<Movie[]>(jsonWithMovies)!;
string userQuestion = "What is the 3 highest rated adventure movies? List their titles, plots and ratings.";

Secrets secrets = SecretManager.GetSecrets();

OpenAIClient openaiClient = new(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);
ChatClient chatClient = openaiClient.GetChatClient(secrets.ModelId);

using var localEmbeddingGenerator = new OnnxLocalEmbeddingGenerator("model.onnx", "vocab.txt");

var vectorStore = new InMemoryVectorStore(new InMemoryVectorStoreOptions
{
    EmbeddingGenerator = localEmbeddingGenerator
});

VectorStoreCollection<Guid, MovieVectorStoreRecord> collection = 
    vectorStore.GetCollection<Guid, MovieVectorStoreRecord>("movies");

await collection.EnsureCollectionExistsAsync();

Utils.WriteLineYellow("Indexing movies locally...");
int counter = 0;
foreach (Movie movie in movieDataForRag)
{
    counter++;
    Console.Write($"\rProcessing: {counter}/{movieDataForRag.Length}");
    
    var embeddings = await localEmbeddingGenerator.GenerateAsync([movie.GetTitleAndDetails()]);
    
    await collection.UpsertAsync(new MovieVectorStoreRecord
    {
        Id = Guid.NewGuid(),
        Title = movie.Title,
        Plot = movie.Plot,
        Rating = movie.Rating,
        Embedding = embeddings.First().Vector 
    });
}
Console.WriteLine("\nIndexing complete.\n");

SearchTool searchTool = new(collection, localEmbeddingGenerator);

var agent = chatClient.CreateCerebrasAgent(
    instructions: "You are an expert on a set of made up movies. ALWAYS use the search_movies tool to find information.",
    name: "CerebrasRAGExpert",
    tools: [AIFunctionFactory.Create(searchTool.SearchVectorStore, "search_movies")]
);

Utils.WriteLineGreen("Asking Cerebras...");
var response = await agent.RunAsync(userQuestion);

Utils.Separator();
Console.WriteLine(response.GetCleanContent());
Utils.Separator();

response.Usage.OutputAsInformation();

class SearchTool(
    VectorStoreCollection<Guid, MovieVectorStoreRecord> collection, 
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    public async Task<List<string>> SearchVectorStore(string query)
    {
        Utils.WriteLineDarkGray($"[Tool Call] Searching for: {query}");
        
        var embeddings = await embeddingGenerator.GenerateAsync([query]);
        var vector = embeddings.First().Vector;

        var searchResult = collection.SearchAsync(
            vector, 
            5, 
            new VectorSearchOptions<MovieVectorStoreRecord>
            { 
                IncludeVectors = false 
            });

        List<string> results = [];
        
        await foreach (var result in searchResult)
        {
            results.Add(result.Record.GetTitleAndDetails());
        }

        return results;
    }
}