namespace AdvancedRAGTechniques.Models;

public class Movie
{
    public required string Title { get; set; }
    public required string Plot { get; set; }
    public required decimal Rating { get; set; }

    public string GetTitleAndDetails() => $"Title: {Title} - Rating: {Rating} - Plot: {Plot}";
}