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

### 3. ConversationThreads

This project focuses on multi-turn conversations and state persistence. It demonstrates how to save an agent's conversation history to a file and restore it later, allowing the agent to "remember" previous interactions across application restarts.

**Key Concepts:**
- Managing the `AgentThread` lifecycle.
- Serializing and Deserializing threads using `agent.Serialize()` and `agent.DeserializeThread()`.
- Implementing a file-based persistence layer (`AgentThreadPersistence`).
- Restoring the console UI state from a resumed thread's message store.

---

### 4. ToolCalling.Basics

This project introduces the fundamental concept of **Tool Calling** (Function Calling). It demonstrates how an agent can extend its capabilities by executing local C# code to fetch real-time information.

**Key Concepts:**
- Defining and registering C# methods as tools using `AIFunctionFactory`.
- The "Tool Call Flow": User Input -> Agent decides to use a tool -> System executes local code -> Agent summarizes the result for the user.
- Using the `tools` parameter in `ChatClientAgent` to provide external capabilities.
- Providing real-time data (like system time and timezone) to a "Time Expert" agent.

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