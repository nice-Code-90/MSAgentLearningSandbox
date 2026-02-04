using Microsoft.Extensions.AI;
using AdvancedRAGTechniques.Models;
using AdvancedRAGTechniques.SearchOptions;
using AdvancedRAGTechniques.EmbeddingOptions;
using Shared.AI;
using Microsoft.Agents.AI;
using AdvancedRAGTechniques.Utils;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using Shared.Extensions;

namespace AdvancedRAGTechniques;

public static class Option1RephraseQuestion
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
            await OriginalEmbedding.Embed(collection, movieData, generator);
        }

        var searchTool = new OriginalSearchTool(collection, generator);

        var agent = chatClient.AsAIAgent(
            instructions: "You are a movie expert. Use tools to find info. When using tools, use only keywords.",
            tools: [AIFunctionFactory.Create(searchTool.SearchVectorStore)]
        ).AsBuilder()
         .Use(Middleware.FunctionCallMiddleware)
         .Build();

        var response = await agent.RunAsync(question.Text);
        Console.WriteLine(response);
        response.Usage.OutputAsInformation(response.Text);
    }
}