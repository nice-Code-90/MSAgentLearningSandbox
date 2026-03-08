using Microsoft.Extensions.Configuration;

namespace Shared;

public class SecretManager
{
    public static Secrets GetSecrets()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .Build();


        string llmAPIKey = config["LlmProvider:ApiKey"] ?? string.Empty;
        string modelId = config["LlmProvider:Model"] ?? "qwen-3-32b";
        string githubPatToken = config["GitHubPatToken"] ?? string.Empty;



        return new Secrets(llmAPIKey, githubPatToken, modelId);
    }
}
