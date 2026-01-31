# Microsoft Agent Framework Learning Sandbox

This repository documents my journey exploring the **Microsoft Agent Framework**. It contains a collection of isolated projects, each focusing on different capabilities, integrations, and patterns within the ecosystem.

> **Acknowledgement:** This repository is inspired by and adapted from [rwjdk/MicrosoftAgentFrameworkSamples](https://github.com/rwjdk/MicrosoftAgentFrameworkSamples). The samples here are rewritten to utilize **Cerebras Inference** with high-performance models.

## Why Cerebras?

Originally, these samples were built with Google Gemini. However, the project has transitioned to **Cerebras** for the following reasons:

- **Extreme Performance:** Cerebras offers the world's fastest inference (2000+ tokens/s), which is crucial for low-latency agentic workflows.
- **Generous Free Tier:** Cerebras provides a superior free access tier for developers, making it ideal for testing complex agentic modeling without immediate billing constraints.
- **OpenAI Compatibility:** Leveraging the `Microsoft.Extensions.AI.OpenAI` library allows for a standard, swappable architecture.

---

## Projects

### 1. ZeroToFirstAgent

This is the "Hello World" of the repository. A simple Console Application that demonstrates how to connect the Microsoft Agent Framework with Cerebras-hosted Llama models using an OpenAI-compatible interface.

**Key Concepts:**

- Setting up `IChatClient` using `Microsoft.Extensions.AI.OpenAI`.
- Configuring custom endpoints for Cerebras (`https://api.cerebras.ai/v1`).
- Creating a basic `ChatClientAgent`.
- Handling the `.AsIChatClient()` extension pattern in .NET 10.

---

### 2. TokenUsage

This project demonstrates how to retrieve and display token usage statistics (Input, Output, and Reasoning tokens) provided by the Cerebras API through the Microsoft Agent Framework.

**Key Concepts:**

- Accessing `UsageDetails` from `AgentRunResponse`.
- Handling token usage in **Streaming** scenarios.
- Monitoring high-speed token generation stats.

---

### 3. ConversationThreads

This project focuses on multi-turn conversations and state persistence. It demonstrates how to save an agent's conversation history to a file and restore it later.

**Key Concepts:**

- Managing the `AgentThread` lifecycle.
- Serializing and Deserializing threads using `agent.Serialize()` and `agent.DeserializeThread()`.
- Implementing a file-based persistence layer (`AgentThreadPersistence`).
- Restoring the console UI state from a resumed thread's message store.

---

### 4. ToolCalling.Basics

This project introduces the fundamental concept of **Tool Calling** (Function Calling). It demonstrates how an agent can execute local C# code.

**Key Concepts:**

- Defining and registering C# methods as tools using `AIFunctionFactory`.
- The "Tool Call Flow": User Input -> Agent decides to use a tool -> System executes local code -> Agent summarizes the result.
- Using the `tools` parameter in `ChatClientAgent` to provide external capabilities.
- Providing real-time data to a "Time Expert" agent.

---

### 5. ToolCalling.Advanced (The "Fruit Sorter")

A complex agentic workflow where the agent manages a local Linux filesystem to sort files based on text descriptions.

**Key Concepts:**

- Multi-step tool execution: `GetFiles`, `GetContentOfFile`, `CreateFolder`, and `MoveFile`.
- Advanced reasoning for data categorization.
- Implementing a `Guard` pattern for directory-locked operations.

---

### 6. ToolCalling.MCP (GitHub Expert)

This project demonstrates the **Model Context Protocol (MCP)** integration, connecting to the remote GitHub Copilot MCP server.

**Key Concepts:**

- Implementing `McpClient` with `HttpClientTransport`.
- Bridging remote MCP tools into the framework as standard `AITools`.
- Securely passing GitHub Personal Access Tokens (PAT) via HTTP headers.

---

### 7. StructuredOutput

This project demonstrates how to force the agent to return data in a specific C# object format using JSON Schemas.

**Key Concepts:**

- **JSON Schema:** Using `ChatResponseFormat.ForJsonSchema<T>` to guide the model.
- **Generic RunAsync:** Utilizing `AgentRunResponse<T>` for automatic deserialization.
- Ensuring type-safety in agentic outputs.

---

### 8. ReasoningModels

Explores how to handle models that perform internal reasoning (thinking) within the framework.

**Key Concepts:**

- **Extended Timeouts:** Configuring `NetworkTimeout` for long-thinking models.
- **Reasoning vs. Response:** Handling the separation between `<think>` blocks and answers.
- **Effort Simulation:** Using instructions and token limits to control reasoning intensity.

---

### 9. AgentInputData (Multi-Modal Inputs)

Explores providing the agent with data other than text, such as Images and PDF files.

**Key Concepts:**

- **UriContent & DataContent:** Standard framework classes for non-text inputs.
- **Provider Constraints:** Testing the limits of Cerebras models with binary data.
- **Local Workarounds:** Why local text extraction is necessary for text-only models.

---

### 10. Workflow.AiAssisted (Pizza Sample)

Demonstrates multi-step agentic workflows with logical branching.

**Key Concepts:**

- **Executors:** Defining isolated units of work (AI or Code-based).
- **WorkflowBuilder:** Mapping the flow from input to final state.
- **Conditional Switching:** Routing the process based on object state (e.g., Stock status).
- **Cerebras Performance:** Executing multi-step AI chains with sub-second latency.
- **Workflow State:** Maintaining and modifying a central object (like PizzaOrder) as it passes through different stages of the process.

---

### 11. IntentDispatcher (The "Router" Pattern)

Demonstrates a sophisticated "Router" architecture using a tiered model approach.

**Key Concepts:**

- **Reasoning-Based Routing:** Using **Qwen-3-32b** as the router because its reasoning stability makes it significantly more reliable for valid JSON generation than smaller 8b models.
- **Tiered Expert Strategy:** Dispatching tasks to **Llama-3.3-70b** for massive domain knowledge (experts) or **Llama-3.1-8b** for fast general responses (Other).
- **Custom Extension Bridge:** Implementation of `RunCerebrasAsync<T>` to bridge the gap between Reasoning models and the framework's strict JSON requirements.
- **Robust Deserialization:** Implementing "forgiving" JSON parsing to handle LLM inconsistencies like case-insensitivity, trailing commas, and String-to-Enum conversion.

---

### 12. MultiAgent.Delegation (Agent-as-a-Tool)

This project demonstrates a hierarchical agent architecture where a "Manager" agent invokes other specialized agents rather than direct code tools to perform tasks.

**Key Concepts:**

- **Agent-as-a-Tool:** Using the `agent.AsAIFunction()` extension to register a full agent as a tool within another agent.
- **Orchestration:** A central `DelegateAgent` orchestrates the workflow, deciding whether to delegate tasks to the `StringAgent` or `NumberAgent`.
- **Middleware Observability:** Implementing a custom `FunctionCallMiddleware` to log real-time details of which agent is calling which tool.
- **Monolith vs. Micro-Agent Pattern:** Comparing a monolithic "Jack of all trades" agent against a delegated, micro-agent structure.

---

### 13. Workflow.Sequential (Legal Summary & Translation)

This project implements a classic two-stage pipeline where the output of the first agent automatically becomes the input of the second agent.

**Key Concepts:**

- **AgentWorkflowBuilder.BuildSequential:** Creating a linear chain where agents work in a strict order.
- **StreamingRun & WatchStreamAsync:** Event-driven monitoring of the entire process and processing intermediate results (events).
- **Agent Specialization:** A "SummaryAgent" is responsible for logical compression, while a "TranslationAgent" handles language transformation.

---

### 14. Workflow.Concurrent (Multi-Perspective Analysis)

This project demonstrates parallel agent execution. The same input data is analyzed simultaneously from multiple different perspectives (legal and spelling).

**Key Concepts:**

- **AgentWorkflowBuilder.BuildConcurrent:** Enables parallel execution of multiple agents. Results become available when all agents have finished.
- **Separated Responsibility:** Agents are unaware of each other, focusing only on the source text.

---

### 15. MultiAgent.Handoff (Dynamic Routing)

This project demonstrates the highest level of agent collaboration, where agents are capable of delegating tasks among themselves.

**Key Concepts:**

- **Dynamic Handoff:** Control transfer based on the agent's decision rather than a predefined sequence.
- **HandoffBuilder:** A specialized builder within the framework that registers transfer possibilities between agents.
- **Cerebras Hybrid Strategy:**
  - **Router:** Qwen-2.5-32b for stable logical decisions and accurate delegation.
  - **Experts:** Llama-3.3-70b for deep domain knowledge and rich responses.
- **Context Continuity:** Conversation context is preserved during the handoff, ensuring the expert already knows what the user asked the router.

---

### 16. SettingsOnAiAgent (Advanced Configuration)

This project explores the extensive configuration surface of the `ChatClientAgent`, demonstrating how to fine-tune agent behavior, metadata, and the underlying execution pipeline.

**Key Concepts:**

- **ChatClientAgentOptions:** Moving beyond simple constructors to use a dedicated options object for defining Name, Description, Instructions, and ChatOptions.
- **Dependency Injection (DI):** Passing an `IServiceProvider` to the agent. This allows tools (AIFunctions) to resolve local services from the .NET DI container during execution.
- **Middleware & Pipeline:** Using the `.AsBuilder()` pattern to inject cross-cutting concerns (Middleware) into the agent's request/response flow.
- **Observability:** Integrating OpenTelemetry to trace agent activities and performance in a standardized way.
- **Client Factories:** Utilizing the `clientFactory` delegate to wrap or customize the underlying `IChatClient` (e.g., adding a `ConfigureOptionsChatClient` to force specific LLM parameters).

## Technical Insights & Learning Outcomes

### The Routing Choice: Qwen vs. Llama

A critical finding during development was that **Llama-3.1-8b** frequently fails at "Structured Output" (JSON) when queries are complex.

- **The Issue:** Small models often include conversational filler or incorrect casing, breaking C# `required` property constraints.
- **The Fix:** Using **Qwen-3-32b** for routing. While it is a reasoning model, its stability with JSON schemas ensures the `switch` logic never fails.

### Tiered Model Performance Table

| Role        | Model           | Why?                                                                       |
| :---------- | :-------------- | :------------------------------------------------------------------------- |
| **Router**  | `qwen-3-32b`    | High precision in following JSON schemas and system instructions.          |
| **Expert**  | `llama-3.3-70b` | Massive knowledge base for specialized, high-quality professional answers. |
| **General** | `llama3.1-8b`   | Lowest latency for simple conversational tasks like greetings.             |

### The JSON vs. Reasoning Tag Conflict

Reasoning Models on Cerebras (like Qwen-3) include `<think>` tags directly in the response content, which breaks the framework's default deserialization.

- **The Solution:** I created a custom extension method `RunCerebrasAsync<T>` that:
  1. Intercepts the raw string response.
  2. Strips the reasoning blocks (`Split("</think>").Last()`).
  3. Cleans up Markdown wrappers (```json).
  4. Manually deserializes the result using a `JsonStringEnumConverter` and case-insensitive options.

### Llama vs. Qwen for Agentic Workflows

During development, I discovered that while Llama is excellent for general chat, **Qwen-3** is significantly more reliable for complex function calling on Cerebras:

- **Llama 3.3:** Occasionally fails with "400 Bad Request" when attempting complex tool chains due to formatting inconsistencies.
- **Qwen 3:** Extremely stable JSON generation and high precision in following system instructions.

### The Multi-Modal Gap on Cerebras

Testing `AgentInputData` revealed that Cerebras models (Llama 3.3, Qwen 3) are currently **text-only**.

- **The 422 Error:** Sending `DataContent` (Images/PDFs) natively results in a `ClientResultException: Status 422 (Unprocessable Entity)`.
- **The Solution:** To work with files on Cerebras, one must implement a local pre-processing layer (e.g., using `PdfPig` for PDF text extraction or Tesseract for OCR) and pass the result to the agent as standard `TextContent`.

### Handling "Reasoning" on Cerebras

Unlike Azure OpenAI (o1), where reasoning tokens are hidden metadata, **Qwen-3-32b** on Cerebras streams thoughts as visible text within `<think>` tags.

- **Output Cleaning:** I implemented string manipulation (`Split("</think>").Last()`) to strip internal reasoning from the user interface.
- **Manual Token Estimation:** Since Cerebras reports 0 reasoning tokens in standard metadata, I created a manual estimation logic that counts words inside `<think>` blocks.
- **Truncation Resilience:** My estimation logic is designed to handle "truncated" responses where the model hits a token limit before closing the `</think>` tag.

### Stability Best Practices

- **Sequential over Parallel:** Setting `AllowMultipleToolCalls = false` is critical on Cerebras to prevent malformed JSON responses during complex operations.
- **Extended Timeouts:** Configuring `NetworkTimeout` to at least 5 minutes is mandatory, as reasoning models can take significant time before the first answer token appears.

### Workflow Instance Lifecycle

In the Microsoft Agent Framework, `Workflow` objects are **single-use**. They cannot be reused for multiple consecutive `RunAsync` calls.

- **The Error:** Attempting to reuse a built `Workflow` instance throws an `InvalidOperationException` (Already Owned).
- **The Pattern:** You must generate a new `Workflow` instance using `AgentWorkflowBuilder` for every single execution (e.g., each chat turn).
- **Reusability:** While the workflow object is disposable, the `ChatClientAgent` instances themselves _can_ be reused across multiple workflow builds without issues.

---

## Getting Started

### Prerequisites

1. **.NET SDK** (version 10)
2. A **Cerebras API Key** ([Cerebras Cloud Console](https://cloud.cerebras.ai/))
3. A **GitHub PAT Token** (for the MCP project)

### Configuration

Create a `appsettings.Local.json` file in the project root:

```json
{
  "Cerebras": {
    "ApiKey": "YOUR_KEY",
    "ModelId": "qwen-3-32b"
  },
  "GitHub": {
    "PatToken": "YOUR_GITHUB_TOKEN"
  }
}
```
