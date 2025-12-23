using OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using Shared.Extensions;
using OpenAI.Chat;

namespace Workflow.AiAssisted.PizzaSample;

/// <summary>
/// Factory for creating agents optimized for the Pizza Workflow using Cerebras models.
/// </summary>
public class AgentFactory(Secrets secrets)
{
    /// <summary>
    /// Creates an agent responsible for parsing natural language into a structured pizza order.
    /// </summary>
    public ChatClientAgent CreateOrderTakerAgent()
    {
        // Using Qwen-3-32b via Cerebras is highly reliable for the structured parsing needed here.
        return GetChatClient()
            .CreateCerebrasAgent(
                instructions: "You are a Pizza Order Taker. Your goal is to parse the user's request into a structured JSON pizza order. Be precise with toppings and sizes.",
                name: "OrderTaker"
            );
    }

    /// <summary>
    /// Creates an agent responsible for explaining availability issues to the customer.
    /// </summary>
    public ChatClientAgent CreateWarningToCustomerAgent()
    {
        return GetChatClient()
            .CreateCerebrasAgent(
                instructions: "You are a Pizza Confirmer. Your job is to politely explain to the user why certain parts of their pizza order cannot be fulfilled due to stock issues.",
                name: "WarningAgent"
            );
    }

    /// <summary>
    /// Private helper to initialize the Cerebras-compatible IChatClient with appropriate timeouts.
    /// </summary>
    private ChatClient GetChatClient()
    {
        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(secrets.CerebrasApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri("https://api.cerebras.ai/v1"),
                // High timeout is mandatory for reasoning models like Qwen-3 during complex workflow steps.
                NetworkTimeout = TimeSpan.FromMinutes(5)
            }
        );

        return openAIClient.GetChatClient(secrets.ModelId);
    }
}