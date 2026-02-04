namespace AdvancedRAGTechniques.Models;

public class IntentResponse
{
    public TypeOfQuestion TypeOfQuestion { get; set; } = TypeOfQuestion.GenericMovieQuestion;

    public string Genre { get; set; } = "All";

    public int NumberOfResults { get; set; } = 3;

    public string? Reasoning { get; set; }
}

public enum TypeOfQuestion
{
    MovieGenreRanking,
    MovieGenreSearch,
    GenericMovieQuestion
}