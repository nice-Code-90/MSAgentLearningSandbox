using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using AdvancedRAGTechniques.Models;
using AdvancedRAGTechniques.SearchOptions;
using AdvancedRAGTechniques.EmbeddingOptions;
using Shared.AI;
using Shared.Extensions;
using Microsoft.Agents.AI;
using AdvancedRAGTechniques.Utils;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AdvancedRAGTechniques;

public static class Option3CommonSense
{
    public static async Task Run(
        bool importData,
        Movie[] movieData,
        ChatMessage question,
        IChatClient chatClient,
        SqliteCollection<Guid, MovieVectorStoreRecord> collection,
        OnnxLocalEmbeddingGenerator generator)
    {
        if (importData)
        {

            await EnhanceDataEmbedding.Embed(chatClient, collection, movieData, generator);
        }


        var intentAgent = chatClient.AsAIAgent(instructions: """
            You are a classification assistant. 
            Analyze the user's movie-related question and respond ONLY with a JSON object.
            NEVER use external movie knowledge or real-world movies (like Lord of the Rings).
            
            Structure:
            {
              "TypeOfQuestion": 0, // 0: MovieGenreRanking, 1: MovieGenreSearch, 2: Generic
              "Genre": "Adventure",
              "NumberOfResults": 3
            }
            """);


        var intent = await intentAgent.RunCerebrasAsync<IntentResponse>(question.Text);


        switch (intent.TypeOfQuestion)
        {
            case TypeOfQuestion.MovieGenreRanking:
                {
                    Shared.Utils.WriteLineYellow($"[Intent] Ranking detected for Genre: {intent.Genre}");


                    string normalizedGenre = string.IsNullOrWhiteSpace(intent.Genre)
                        ? ""
                        : char.ToUpper(intent.Genre[0]) + intent.Genre.Substring(1).ToLower();

                    List<MovieVectorStoreRecord> matchingMovies = [];


                    var dummyVector = new float[384];
                    var searchOptions = new VectorSearchOptions<MovieVectorStoreRecord>
                    {
                        IncludeVectors = false,
                        Filter = record => record.Genre == normalizedGenre
                    };


                    var searchResult = collection.SearchAsync(dummyVector, int.MaxValue, searchOptions);

                    await foreach (var res in searchResult)
                    {
                        matchingMovies.Add(res.Record);
                    }


                    var topMovies = matchingMovies
                        .OrderByDescending(x => x.Rating)
                        .Take(intent.NumberOfResults)
                        .ToArray();


                    var presenter = chatClient.AsAIAgent(
                        instructions: $"""
                        You are an expert on a set of fictional movies provided to you.
                        DO NOT use any real-world knowledge.
                        Present the data for the user's question: '{question.Text}'
                        Be concise and friendly.
                        """);


                    var response = await presenter.RunAsync(string.Join(" | ", topMovies.Select(x => x.GetTitleAndDetails())));

                    Console.WriteLine(response.GetCleanContent());


                    response.Usage.OutputAsInformation(response.Text);
                    break;
                }

            case TypeOfQuestion.MovieGenreSearch:
            case TypeOfQuestion.GenericMovieQuestion:
            default:
                {

                    var tool = new EnhancedSearchTool(collection, generator);
                    var agent = chatClient.AsAIAgent(
                        instructions: """
                        You are an expert on fictional movies. 
                        Use the provided tools to find movies. 
                        Do not consider real-world movies.
                        List titles, plots, and ratings.
                        """,
                        tools: [AIFunctionFactory.Create(tool.SearchVectorStore)])
                        .AsBuilder().Use(Middleware.FunctionCallMiddleware).Build();

                    var response = await agent.RunAsync(question.Text);

                    Console.WriteLine(response.GetCleanContent());


                    response.Usage.OutputAsInformation(response.Text);
                    break;
                }
        }
    }
}