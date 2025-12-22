# Microsoft Agent Framework Learning Sandbox

This repository documents my journey exploring the **Microsoft Agent Framework**. It contains a collection of isolated projects, each focusing on different capabilities, integrations, and patterns within the ecosystem.

> **Acknowledgement:** This repository is inspired by and adapted from [rwjdk/MicrosoftAgentFrameworkSamples](https://github.com/rwjdk/MicrosoftAgentFrameworkSamples). The samples here are rewritten to utilize **Cerebras Inference** with high-performance models.

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

### 5. ToolCalling.Advanced (The "Fruit Sorter")
A complex agentic workflow where the agent manages a local Linux filesystem. It demonstrates high-level reasoning and multi-step tool execution.

**Key Challenges & Solutions:**
- **Model Selection:** Transitioned from Llama to **Qwen-3-32b** for superior tool-calling stability and reasoning capabilities.
- **Sequential Execution:** Configured `AllowMultipleToolCalls = false` to prevent API errors (400 Bad Request) caused by overwhelming parallel JSON generations.
- **Reasoning Patterns:** Utilizing Qwen's internal `<think>` process to categorize data (e.g., sorting fruits by color based on text descriptions).
- **Filesystem Sandbox:** Implementing a `Guard` pattern to ensure the agent only operates within a specific directory.

---

## Learning Outcomes: Llama vs. Qwen for Agents

During development, I discovered that while Llama is excellent for general chat, **Qwen-3** is significantly more reliable for complex function calling on the Cerebras platform. 
- **Llama 3.3:** Occasionally fails with "400 Bad Request" when attempting complex tool chains due to formatting inconsistencies.
- **Qwen 3:** Extremely stable JSON generation and follows system instructions with high precision, making it the preferred model for "Agentic" workflows in this sandbox.

---

### 6. ToolCalling.MCP (GitHub Expert)
This project demonstrates the **Model Context Protocol (MCP)** integration. The agent connects to the remote GitHub Copilot MCP server to perform real-time repository analysis and management.

**Key Concepts:**
- Implementing `McpClient` with `HttpClientTransport`.
- Bridging remote MCP tools into the Microsoft Agent Framework as standard `AITools`.
- Using **Qwen-3-32b** on Cerebras to handle complex, remote API-driven tool calls.
- Securely passing GitHub Personal Access Tokens (PAT) via additional HTTP headers.

### 7. StructuredOutput
This project demonstrates how to force the agent to return data in a specific C# object format instead of plain text.

**Key Concepts:**
- **JSON Schema:** Using `ChatResponseFormat.ForJsonSchema<T>` to guide the model.
- **Generic RunAsync:** Utilizing `AgentRunResponse<T>` for automatic deserialization.
- **Reliability:** Why Qwen-3-32b is the ideal choice for structured data due to its strict adherence to provided schemas.

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