# Microsoft Agent Framework Learning Sandbox

This repository documents my journey exploring the **Microsoft Agent Framework**. It contains a collection of isolated projects, each focusing on different capabilities, integrations, and patterns within the ecosystem.

> **Acknowledgement:** This repository is inspired by and adapted from [rwjdk/MicrosoftAgentFrameworkSamples](https://github.com/rwjdk/MicrosoftAgentFrameworkSamples). The samples here are rewritten to utilize **Cerebras Inference** with high-performance models.


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

### 17. RAG.LocalEmbeddings (High-Performance Movie Expert)

This project implements a production-grade RAG (Retrieval-Augmented Generation) pattern that combines local, high-speed vectorization with cloud-based reasoning. It is designed as a template for low-latency, privacy-focused semantic search.

**Key Concepts:**

- **Local ONNX Embedding:** Custom `IEmbeddingGenerator` implementation using `Microsoft.ML.OnnxRuntime`.
- **Specific Model: all-MiniLM-L6-v2:** The project utilizes the Sentence-Transformers all-MiniLM-L6-v2 model exported to ONNX format.
  - **Architecture:** A distilled version of BERT, optimized for sentence embeddings.
  - **Dimensions:** 384-dimensional dense vectors.
  - **Sequence Length:** Supports up to 256 tokens.
  - **Efficiency:** Provides a 5x speedup over larger models with minimal loss in retrieval accuracy for general-purpose text.
- **MEVD (Microsoft Extensions Vector Data):** Utilizing the latest .NET 10 abstractions, specifically the `VectorStoreCollection` abstract base class.
- **Hybrid RAG Flow:** Semantic search is performed locally in memory, passing only the most relevant results to Cerebras for final processing.

**Technical Specifications (for LLM Context):**

- **Embedding Provider:** Local ONNX Runtime.
- **Model ID:** `sentence-transformers/all-MiniLM-L6-v2`.
- **Output Vector Size:** 384.
- **Distance Metric:** Cosine Similarity (defined in `MovieVectorStoreRecord`).
- **Required Files:** `model.onnx` (weights) and `vocab.txt` (WordPiece tokenizer vocabulary).

### 18. RAG.AdvancedTechniques (Multi-Tiered Optimization)

This project demonstrates the evolution of RAG (Retrieval-Augmented Generation) through three distinct implementation levels. The goal is to prove how combining "raw" vector search with "Common Sense" agentic logic leads to drastically more accurate and cost-effective results.

**Key Concepts:**

- **Option 1: Query Rephrasing:** The AI rephrases the user's question into keywords to improve the hit rate of semantic search (Similarity Search).
- **Option 2: Metadata Filtering (Hybrid RAG):** Combining vector search with hard SQL filters (e.g., `record.Genre == genre`). This guarantees 100% accuracy in cases where narrowing down to specific categories (e.g., only adventure movies) is required.
- **Option 3: Common Sense (Agentic RAG):** The most advanced level, where an Intent Agent decides the strategy:
  - **Ranking:** If the user requests a top list, the system does not "search" but retrieves all items in the category, and C# code performs the mathematical sorting (based on Rating).
  - **Search:** If the question is more nuanced, it switches back to semantic search.
- **Top-K Optimization (The Lean RAG):** Radical reduction of token costs (from 1300+ to ~300 input tokens) by tightening retrieval limits (e.g., passing only the best 20 candidates to the AI instead of 100).
- **Normalization Layer:** Handling case-sensitivity and cleaning JSON responses generated by LLMs for stable deserialization.

### 19. Debug.DeepDive (Raw Request/Response Monitoring)

This project makes the communication exchange visible at the network layer. It demonstrates how to "intercept" and visualize the raw JSON communication between the Microsoft Agent Framework and the Cerebras API.

**Key Concepts:**

- **Custom HttpClientHandler:** Implementing a custom middleware that intercepts the outgoing `HttpRequestMessage` and the incoming `HttpResponseMessage`.
- **Pipeline Injection:** Utilizing `HttpClientPipelineTransport` within the `OpenAIClient` configuration to insert our custom logger directly into the SDK's execution chain.
- **Observability:** Inspecting Cerebras-specific JSON metadata (such as `time_info` and precise response timings) that are not always available in standard framework objects.
- **Stable Token Estimation:** A fallback logic in `UsageDetailsExtensions` that estimates reasoning tokens from the message text when the SDK bridge fails to map them.

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

### Local vs. Remote Embeddings (The Latency Win)

Development proved that combining local ONNX embedding generation with Cerebras’ extreme inference speed creates a RAG experience where total response time is often lower than the time a traditional cloud provider takes just to generate vectors.

### The MEVD Abstract Base Class Pattern

In the .NET 10 AI ecosystem, the shift from interfaces (like `IVectorStoreRecordCollection`) to abstract base classes (`VectorStoreCollection`) provides more stable type-tracking and better default behavior but requires updating older Semantic Kernel documentation patterns.

### The Metadata "Black Box" in Preview SDKs

During development, it was discovered that the current preview versions of `Microsoft.Extensions.AI` and the OpenAI SDK bridge do not always populate the `UsageDetails` dictionary with Cerebras-specific metadata (like the nested `reasoning_tokens` object).

- **The Limitation:** The `AgentResponse.ToString()` method only returns the text content of the message; it does not expose technical metadata such as usage or timing.
- **The Solution:** The `Debug.DeepDive` project implements a two-tier strategy to retrieve reasoning data:
  1. **Direct Mapping:** The system first attempts to find the data in the official `AdditionalCounts` dictionary using known keys (e.g., `reasoning_tokens`).
  2. **Stable Estimation:** If the API reports 0 (a common occurrence with the current Cerebras/OpenAI bridge), the system falls back to a word-count-based estimation.
  3. **The Formula:** By applying a `word_count * 1.33` multiplier to the text found inside `<think>` tags, we achieve a remarkably stable approximation (~8% error margin) of the actual token count.

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
