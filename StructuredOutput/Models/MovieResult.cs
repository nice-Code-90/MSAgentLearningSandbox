#pragma warning disable CA1819

namespace StructuredOutput.Models;

public class MovieResult
{
    public required string MessageBack { get; set; }
    public required Movie[] Top10Movies { get; set; }
}