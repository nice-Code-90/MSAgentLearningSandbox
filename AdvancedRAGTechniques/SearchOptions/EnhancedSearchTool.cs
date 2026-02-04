using Microsoft.Extensions.VectorData;
using Shared.AI;
using AdvancedRAGTechniques.Models;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AdvancedRAGTechniques.SearchOptions;

public class EnhancedSearchTool(

    SqliteCollection<Guid, MovieVectorStoreRecord> collection,
    OnnxLocalEmbeddingGenerator generator)
{
    public async Task<List<string>> SearchVectorStore(string question, string genre)
    {

        var embeddingResult = await generator.GenerateAsync([question]);
        var vector = embeddingResult.First().Vector;
        string normalizedGenre = string.IsNullOrWhiteSpace(genre)
        ? ""
        : char.ToUpper(genre[0]) + genre.Substring(1).ToLower();

        var options = new VectorSearchOptions<MovieVectorStoreRecord>
        {
            IncludeVectors = false,
            Filter = record => record.Genre == normalizedGenre
        };


        var searchResult = collection.SearchAsync(vector, 10, options);
        List<string> output = [];
        await foreach (var res in searchResult)
        {
            output.Add(res.Record.GetTitleAndDetails());
        }

        return output;
    }
}