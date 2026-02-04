using Microsoft.Extensions.VectorData;
using Shared.AI;
using AdvancedRAGTechniques.Models;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AdvancedRAGTechniques.SearchOptions;

public class OriginalSearchTool(SqliteCollection<Guid, MovieVectorStoreRecord> collection, OnnxLocalEmbeddingGenerator generator)
{
    public async Task<List<string>> SearchVectorStore(string question)
    {
        var embedding = await generator.GenerateAsync([question]);
        var results = collection.SearchAsync(embedding.First().Vector, 10, new() { IncludeVectors = false });
        List<string> output = [];
        await foreach (var res in results) output.Add(res.Record.GetTitleAndDetails());
        return output;
    }
}