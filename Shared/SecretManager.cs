using Microsoft.Extensions.Configuration;

namespace Shared;

public class SecretManager
{
    public static Secrets GetSecrets()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .Build();


        string cerebrasKey = config["Cerebras:ApiKey"] ?? string.Empty;
        string modelId = config["Cerebras:Model"] ?? "qwen-3-32b";
        string githubPatToken = config["GitHubPatToken"] ?? string.Empty;



        return new Secrets(cerebrasKey, githubPatToken, modelId);
    }
}
