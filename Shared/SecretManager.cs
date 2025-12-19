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


        string cerebrasKey = config["Cerebras:ApiKey"]
            ?? throw new InvalidOperationException("Cerebras API Key not found in appsettings.");

        string googleKey = config["GenerativeAI:ApiKey"] ?? string.Empty;

        return new Secrets(googleKey, cerebrasKey);
    }
}
