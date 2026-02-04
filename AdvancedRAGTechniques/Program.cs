using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using OpenAI;
using OpenAI.Chat;
using Shared;
using Shared.AI;
using System.ClientModel;
using System.Text.Json;
using AdvancedRAGTechniques;
using AdvancedRAGTechniques.Models;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Console.Clear();

string jsonWithMovies = await File.ReadAllTextAsync("made_up_movies.json");
Movie[] movieDataForRag = JsonSerializer.Deserialize<Movie[]>(jsonWithMovies)!;

Secrets secrets = SecretManager.GetSecrets();

OpenAIClient openaiClient = new(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
);

ChatClient sdkChatClient = openaiClient.GetChatClient(secrets.ModelId);
IChatClient chatClient = sdkChatClient.AsIChatClient();

using var embeddingGenerator = new OnnxLocalEmbeddingGenerator("model.onnx", "vocab.txt");

string connectionString = "Data Source=movies.db";
var vectorStore = new SqliteVectorStore(connectionString);

SqliteCollection<Guid, MovieVectorStoreRecord> collection =
    vectorStore.GetCollection<Guid, MovieVectorStoreRecord>("movies");

await collection.EnsureCollectionExistsAsync();

bool importData = false;
var firstCheck = await collection.GetAsync(Guid.NewGuid());
if (firstCheck == null)
{
    Shared.Utils.WriteLineYellow("Database check: Re-import data? (y/n)");
    var key = Console.ReadKey(true);
    if (key.Key == ConsoleKey.Y) importData = true;
}

ChatMessage question = new(ChatRole.User, "What is the 3 highest rated adventure movies?");

//Shared.Utils.Separator();
//await Option1RephraseQuestion.Run(importData, movieDataForRag, question, chatClient, collection, embeddingGenerator);

Shared.Utils.Separator();
await Option2EnhanceEmbeddings.Run(importData, movieDataForRag, question, chatClient, collection, embeddingGenerator);

//Shared.Utils.Separator();
//await Option3CommonSense.Run(importData, movieDataForRag, question, chatClient, collection, embeddingGenerator);