# Microsoft Agent Framework Learning Sandbox

This repository documents my journey exploring the **Microsoft Agent Framework**. It contains a collection of isolated projects, each focusing on different capabilities, integrations, and patterns within the ecosystem.

> **Acknowledgement:** This repository is inspired by and adapted from [rwjdk/MicrosoftAgentFrameworkSamples](https://github.com/rwjdk/MicrosoftAgentFrameworkSamples). The samples here are rewritten to utilize **Cerebras Inference** with Llama models.

## Why Cerebras?

Originally, these samples were built with Google Gemini. However, the project has transitioned to **Cerebras** for the following reasons:

* **Extreme Performance:** Cerebras offers the world's fastest inference (2000+ tokens/s), which is crucial for low-latency agentic workflows.
* **Generous Free Tier:** Cerebras provides a superior free access tier for developers, making it ideal for testing complex agentic modeling without immediate billing constraints.
* **OpenAI Compatibility:** Leveraging the `Microsoft.Extensions.AI.OpenAI` library allows for a standard, swappable architecture.

---

## Projects

### 1. ZeroToFirstAgent

This is the "Hello World" of the repository. A simple Console Application that demonstrates how to connect the Microsoft Agent Framework with Cerebras-hosted Llama models using an OpenAI-compatible interface.

**Key Concepts:**
- Setting up `IChatClient` using `Microsoft.Extensions.AI.OpenAI`.
- Configuring custom endpoints for Cerebras (`https://api.cerebras.ai/v1`).
- Creating a basic `ChatClientAgent`.
- Handling the `.AsIChatClient()` extension pattern in .NET 10.

### 2. TokenUsage

This project demonstrates how to retrieve and display token usage statistics (Input, Output, and Reasoning tokens) provided by the Cerebras API through the Microsoft Agent Framework.

**Key Concepts:**
- Accessing `UsageDetails` from `AgentRunResponse`.
- Handling token usage in **Streaming** scenarios.
- Monitoring high-speed token generation stats.

---

## Getting Started

### Prerequisites
1. **.NET SDK** (version 10)
2. A **Cerebras API Key** (Get it from [Cerebras Cloud Console](https://cloud.cerebras.ai/))

### Configuration Pattern

Most projects in this repository use a **layered configuration** approach to keep API keys secure.

1. **Clone the repository.**
2. **Navigate to the specific project folder** (e.g., `ZeroToFirstAgent.Cerebras`).
3. **Create a `appsettings.Local.json` file** in the project root:
   ```json
   {
     "Cerebras": {
       "ApiKey": "YOUR_ACTUAL_CEREBRAS_API_KEY_HERE"
     }
   }