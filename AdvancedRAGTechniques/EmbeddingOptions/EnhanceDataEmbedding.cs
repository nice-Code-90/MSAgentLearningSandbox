using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Shared.AI;
using Shared.Extensions;
using AdvancedRAGTechniques.Models;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AdvancedRAGTechniques.EmbeddingOptions;

public static class EnhanceDataEmbedding
{
    public static async Task Embed(
        IChatClient chatClient,
        SqliteCollection<Guid, MovieVectorStoreRecord> collection,
        Movie[] movieData,
        OnnxLocalEmbeddingGenerator generator)
    {
        var genreAgent = chatClient.AsAIAgent(instructions: "Pick one: Adventure, Sci-Fi, Comedy, Horror, Action, Romance. Output ONLY the word.");

        await collection.EnsureCollectionExistsAsync();

        int counter = 0;
        foreach (var movie in movieData)
        {
            var genreRes = await genreAgent.RunAsync($"Determine genre for: {movie.Title} - {movie.Plot}");
            string genre = genreRes.GetCleanContent().Trim();

            // Helyi embedding generálás
            var embeddingResult = await generator.GenerateAsync([movie.GetTitleAndDetails()]);

            await collection.UpsertAsync(new MovieVectorStoreRecord
            {
                Id = Guid.NewGuid(),
                Title = movie.Title,
                Plot = movie.Plot,
                Rating = (double)movie.Rating,
                Genre = genre,
                Embedding = embeddingResult.First().Vector
            });

            counter++;
            Console.Write($"\rEmbedding with Genre: {counter}/{movieData.Length}");
        }
        Console.WriteLine("\nEnhanced embedding complete.");
    }
}