using AdvancedRAGTechniques.Models;
using Shared.AI;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AdvancedRAGTechniques.EmbeddingOptions;

public static class OriginalEmbedding
{
    public static async Task Embed(
        // JAVÍTÁS: A típus neve itt is a hosszú változat
        SqliteCollection<Guid, MovieVectorStoreRecord> collection,
        Movie[] movieData,
        OnnxLocalEmbeddingGenerator generator)
    {
        // JAVÍTÁS: SQLite-specifikus metódusnév
        await collection.EnsureCollectionExistsAsync();

        int counter = 0;
        foreach (var movie in movieData)
        {
            var embedding = await generator.GenerateAsync([movie.GetTitleAndDetails()]);
            await collection.UpsertAsync(new MovieVectorStoreRecord
            {
                Id = Guid.NewGuid(),
                Title = movie.Title,
                Plot = movie.Plot,
                Rating = (double)movie.Rating,
                Embedding = embedding.First().Vector
            });
            Console.Write($"\rEmbedding: {++counter}/{movieData.Length}");
        }
        Console.WriteLine("\nBasic embedding complete.");
    }
}