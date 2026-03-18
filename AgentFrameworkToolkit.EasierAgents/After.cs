using AgentFrameworkToolkit.Cerebras;
using AgentFrameworkToolkit.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;

namespace AgentFrameworkToolkit.EasierAgents;

public class After
{
    public static async Task RunAsync()
    {
        //Weather-Task: With any Cerebras model, Call Tool with middleware and return as structured Output

        Secrets secrets = SecretManager.GetSecrets();
        string apiKey = secrets.LLMApiKey;
        string modelId = secrets.ModelId;

        CerebrasAgentFactory agentFactory = new(apiKey);

        CerebrasAgent agent = agentFactory.CreateAgent(new AgentOptions
        {
            Model = modelId,
            Tools = [AIFunctionFactory.Create(WeatherTool.GetWeather)],
            RawToolCallDetails = details => { Utils.WriteLineDarkGray(details.ToString()); }
        });

        AgentResponse<WeatherReport> response = await agent.RunAsync<WeatherReport>("What is the Weather like in Paris");
        WeatherReport weatherReport = response.Result;
        Console.WriteLine("City: " + weatherReport.City);
        Console.WriteLine("Condition: " + weatherReport.Condition);
        Console.WriteLine("Degrees: " + weatherReport.Degrees);
        Console.WriteLine("Fahrenheit: " + weatherReport.Fahrenheit);
        response.Usage.OutputAsInformation();
    }
}