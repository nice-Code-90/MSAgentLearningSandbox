using OpenAI;
using System.ClientModel;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;
using System.Text.Json;
using System.ClientModel.Primitives;

Console.Clear();

Secrets secrets = SecretManager.GetSecrets();
string cerebrasEndpoint = "https://api.cerebras.ai/v1";

using var handler = new CustomClientHttpHandler();
using var httpClient = new HttpClient(handler);

OpenAIClient openaiClient = new(
    new ApiKeyCredential(secrets.CerebrasApiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(cerebrasEndpoint),
        Transport = new HttpClientPipelineTransport(httpClient)
    }
);

IChatClient chatClient = openaiClient.GetChatClient(secrets.ModelId).AsIChatClient();

var agent = chatClient.CreateCerebrasAgent(
    instructions: "You are a helpful assistant. Always respond in JSON format.",
    name: "Cerebras-Debug-Agent"
);

try
{
    Utils.WriteLineYellow($"Sending request to Cerebras ({secrets.ModelId})...");
    Utils.Separator();

    var response = await agent.RunAsync("Say hello and tell me one fun fact about light speed in a short JSON object.");

    Utils.WriteLineGreen("\n[Final Answer (Cleaned)]");
    Console.WriteLine(response.GetCleanContent());

    Utils.Separator();
    response.Usage?.OutputAsInformation(response.ToString());
}
catch (Exception ex)
{
    Utils.WriteLineRed(ex);
}

class CustomClientHttpHandler() : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            string requestString = await request.Content.ReadAsStringAsync(cancellationToken);
            Utils.WriteLineGreen($"Raw Request to: {request.RequestUri}");
            Utils.WriteLineDarkGray(MakePretty(requestString));
            Utils.Separator();
        }


        var response = await base.SendAsync(request, cancellationToken);

        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        Utils.WriteLineGreen("Raw Response from Cerebras:");
        Utils.WriteLineDarkGray(MakePretty(responseString));
        Utils.Separator();

        return response;
    }

    private string MakePretty(string input)
    {
        try
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(input);
            return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return input;
        }
    }
}