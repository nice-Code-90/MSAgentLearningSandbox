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

        string googleGeminiApiKey = config["GenerativeAI:ApiKey"]
            ?? throw new InvalidOperationException("API Key not found. Please check appsettings.Local.json");

        return new Secrets(googleGeminiApiKey);
    }
}
