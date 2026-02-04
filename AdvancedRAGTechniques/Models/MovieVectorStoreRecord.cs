using Microsoft.Extensions.VectorData;

namespace AdvancedRAGTechniques.Models;

public class MovieVectorStoreRecord
{
    [VectorStoreKey] public required Guid Id { get; set; }
    [VectorStoreData] public required string Title { get; set; }
    [VectorStoreData] public required string Plot { get; set; }
    [VectorStoreData] public required double Rating { get; set; }
    [VectorStoreData] public string? Genre { get; set; }

    [VectorStoreVector(384, DistanceFunction = DistanceFunction.EuclideanDistance, IndexKind = IndexKind.Hnsw)]
    public ReadOnlyMemory<float>? Embedding { get; set; }

    public string GetTitleAndDetails() => $"Title: {Title} [Genre: {Genre}] - Rating: {Rating} - Plot: {Plot}";
}