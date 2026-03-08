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

### 20. ChatHistory.Reducers (Context Optimization)

This project demonstrates how to keep costs and the model context window under control during long conversations without the agent losing the thread.

**Key Concepts:**

- **IChatReducer Interface:** The framework's abstraction for filtering and compressing conversation history.
- **MessageCountingChatReducer (The "Raw Cutoff"):** A simple counter-based technique that ruthlessly deletes the oldest messages when the `targetCount` is reached.
- **SummarizingChatReducer (The "Intelligent Memory"):** A more advanced approach where the agent summarizes old messages via a background call, preserving important facts (e.g., the user's name) while consuming fewer tokens.
- **ChatHistoryProviderFactory:** The agent's configuration point where we inject the chosen reduction strategy into the agent's lifecycle using `InMemoryChatHistoryProvider`.

### 21. MultiAgent.A2A (Remote Agent-as-a-Tool)

This project explores the **Agent-to-Agent (A2A)** protocol, a standard for cross-agent communication. It demonstrates a distributed architecture where a local "Manager" agent discovers and invokes a remote "Specialist" agent over the network.

**Key Concepts:**

- **Agent Card:** Using `AgentCard` to publish an agent's "business card"—defining its name, version, and specialized skills via a well-known JSON endpoint.
- **Remote Discovery:** Implementing `A2ACardResolver` to dynamically discover and instantiate a remote `AIAgent` from a URI.
- **Network Transparency:** Wrapping a remote agent as a local tool using `remoteAgent.AsAIFunction()`, allowing the client to delegate tasks without knowing the underlying implementation.
- **ASP.NET Core Hosting:** Using `MapA2A` and `MapWellKnownAgentCard` to host an agent as a web service.
- **Cross-Process Execution:** Demonstrating the flow between a Server process (hosting FileTools) and a Client process (orchestrating user intent) using Cerebras for high-speed reasoning.

### 22. MultiAgent.AgUI (Frontend-Backend Interaction)

This project explores the **Agent User Interaction (AG-UI)** protocol, a specialized communication standard designed to bridge the gap between AI backends and interactive frontends (e.g., Blazor, React, or Console UIs). It solves the complex problem of managing real-time streaming and client-side capability exposure over standard HTTP.

**Key Concepts:**

- **Decoupled Architecture:** Separating the "Brain" (Backend Agent) from the "Interface" (Frontend Client) using a standardized protocol instead of custom, fragile WebSockets or Server-Side Events (SSE) implementations.
- **Client-Side Tool Execution:** A revolutionary pattern where the frontend can expose its own local functions (e.g., `ChangeUIColor`) to a remote backend agent. The agent "sees" these as available tools, and the protocol handles the round-trip execution automatically.
- **AgUI Chat Client Proxy:** Implementing a specialized client that acts as a proxy, forwarding user messages and tool definitions to the server without requiring API keys or LLM credentials on the frontend.
- **Streaming over HTTP:** Simplified handling of real-time token streaming and UI updates using the built-in AG-UI event stream.
- **Proxy Agent Constraints:** Understanding the "Shell" nature of AgUI proxy agents, where the client must manually manage conversation threads and system instructions during the current preview phase.

### 23. DevUI.Intro (Visualizing Agents & Workflows)

This project demonstrates the usage of **DevUI**, an ASP.NET Core-based developer interface for real-time testing and visualization of agents and workflows.

**Key Concepts:**

- **Developer Dashboard:** Activating the `MapDevUI` endpoint, which provides a local web interface for chatting with agents and monitoring the system's internal state.
- **OpenAI Protocol Bridge:** Registering the `AddOpenAIResponses` and `AddOpenAIConversations` services, enabling the framework to communicate with DevUI in a standardized manner.
- **Workflow Visualization:** Graphical representation of sequential and concurrent workflows, allowing live tracking of which agent is currently working on a specific task.
- **Agent Registry Integration:** Demonstrating how to register different types of agents (simple "dummy" agents or complex `AIAgent` instances) into the central DI container, making them available in the dropdown menu.

### 24. MultiAgent.AgUI.Advanced (Frontend-Backend Orchestration)

This project explores the advanced implementation of the Agentic User Interaction (AG-UI) protocol, moving beyond simple chat to handle complex, distributed agentic workflows between a Blazor WebAssembly frontend and a .NET Backend.

**Key Concepts:**

- **Streaming-by-Default Architecture:** A critical discovery in the AG-UI protocol is that everything is a stream. Even if the client calls `RunAsync`, the framework transparently converts it to `RunStreamingAsync` behind the scenes.
- **Client-Side Capability Injection:** The frontend (Blazor) can expose its own local functions (like `ChangeColor`) as tools. The backend agent "sees" these tools and can invoke them across the network to manipulate the user interface directly.
- **Structured Output "Hoops":** Standard structured output (returning a C# object) is not natively supported in AG-UI's default streaming mode. This project implements a Custom Agent Wrapper that intercepts tool results and packages them into `DataContent` objects within the stream.
- **Credential-Free Frontend:** By using the AG-UI proxy, the frontend requires no API keys or LLM credentials. All sensitive authentication (Cerebras keys) remains securely on the backend server.
- **Aspire Orchestration:** Utilizes the .NET Aspire AppHost to manage the complex networking and port synchronization between the server and the WASM client.

### 25. AgentInput.TOON (Token-Efficient Data Representation)

This project demonstrates how to use the Token-Orientated Object Notation (TOON) format to drastically reduce token consumption when passing large datasets to agents through tools.

**Key Concepts:**

- **TOON vs. JSON:** Comparing the verbosity of standard JSON (where keys are repeated for every record) against the lean TOON structure (where keys are defined once as a header).
- **ToonNet Integration:** Using the ToonNet NuGet package to serialize C# object lists into token-efficient strings.
- **Token Optimization:** Measuring the significant reduction in input tokens (up to 40-60% savings) when providing the agent with extensive reference data.
- **Retrieval Accuracy:** Observing how LLMs often parse flat TOON structures with higher precision than deeply nested JSON.

### 26. DurableAgents.AzureFunctions (Stateful Cloud Hosting)

This project demonstrates how to host agents within Azure Functions using the `Microsoft.Agents.AI.Hosting.AzureFunctions` extension. It moves beyond local console apps to a production-ready, serverless architecture where agent state is automatically managed by the infrastructure.

**Key Concepts:**

- **Durable Agents:** Leveraging the Durable Task Framework to maintain agent state across multiple HTTP calls without manual serialization code.
- **Azure Functions Integration:** Registering agents using `builder.ConfigureDurableAgents` in a dotnet-isolated worker process.
- **Cloud-Native Persistence:** Automatically storing conversation threads and history in Azure Storage (Blobs, Queues, and Tables) via the Durable Functions backend.
- **Serverless Orchestration:** Handling long-running agentic tasks in a scalable, event-driven environment while maintaining context.

### 27. LocalLLM.ONNX (Offline Phi-4-mini)

This project demonstrates how to run a Small Language Model (SLM) completely offline, without any external services or API keys, using the ONNX format.

**Key Concepts:**

- **Hugging Face CLI:** Utilizing the `huggingface_hub` to download specific model variants (e.g., `microsoft/Phi-4-mini-instruct-onnx`).
- **OnnxRuntimeGenAIChatClient:** Initializing the dedicated chat client by pointing it to a local folder containing the model weights and configurations.
- **Zero-Dependency AI:** Executing AI calls using only local hardware (CPU/GPU) with no requirement for background services like Ollama.
- **.AsAIAgent() Pattern:** Converting the specialized ONNX client into a standard `ChatClientAgent` to maintain framework consistency.

### 28. UserMemory.CustomProvider (Persistent Facts)

This project demonstrates long-term retention of user data using a custom `AIContextProvider`. It allows the agent to "remember" specific user details (name, preferences, location) even after the application is restarted and the conversation thread is cleared.

**Key Concepts:**

- **AIContextProvider:** The framework's central interface for injecting dynamic context into every LLM call.
- **ProvideAIContextAsync:** Automatically prepends saved facts to the System Instructions, ensuring the agent is always aware of the user's background.
- **StoreAIContextAsync:** A post-response hook that analyzes the latest exchange to extract and update new "memories."
- **Dual-Agent Strategy:** Using a dedicated, cost-effective model (Llama-3.1-8b) specifically for memory extraction, keeping the main agent focused on the primary task.
- **File-based Persistence:** Simulating a database using local .txt files identified by a unique `userId`.

### 29. TextToSpeech.Groq (Low-Latency Voice)

This project demonstrates the integration of high-performance Text-to-Speech (TTS) capabilities using Groq's low-latency inference engine. It focuses on bridging the gap between OpenAI-compatible audio clients and specialized provider-specific models.

**Key Concepts:**

- **OpenAI-Compatible TTS:** Utilizing the `AudioClient` with custom endpoints (`api.groq.com`) to generate human-like speech.
- **Model Constraints:** Handling the specific requirements of the `canopylabs/orpheus-v1-english` model, such as the mandatory `wav` response format.
- **Raw PCM Playback:** Implementing `RawSourceWaveStream` from the NAudio library to play back headerless (raw) audio data that lacks the standard RIFF/WAV header.
- **Audio Specifications:** Configuring `WaveFormat` (e.g., 24kHz, 16-bit, Mono) to match the internal sample rate of the inference model for accurate pitch and speed.
- **Provider Handshaking:** Navigating `ClientResultException` scenarios, specifically the `model_terms_required` error, which necessitates manual acceptance of terms on the provider's console before API activation.

### 30. ToolCalling.ServiceInjection (Advanced DI Pattern)

This project demonstrates how to handle complex dependencies within AI tools using the framework's built-in service injection capabilities.

**Key Concepts:**

- **Method-Level DI:** Defining tools that accept `IServiceProvider` as a parameter.
- **Service Registration:** Passing the `serviceProvider` to the agent via the `services` parameter during the `.AsAIAgent()` call.
- **Hybrid Tool Management:** Simultaneous use of static methods, instance methods, and classes resolved via DI (e.g., `HttpClient`).
- **Exception Prevention:** How to avoid runtime errors when a tool requires an external resource that the agent should resolve on its own.

### 31. VoiceToText.InteractionLoop (Hands-Free AI)

This project implements a full voice-driven interaction loop. It demonstrates the coordination between specialized audio transcription models and reasoning models to create a fluid, hands-free AI experience using high-speed inference.

**Key Concepts:**

- **Real-time Audio Capture:** Implementing a `WaveInEvent` recording loop with NAudio to capture microphone input directly into a `MemoryStream` for immediate cloud processing.
- **Audio Transcription (STT):** Utilizing the `whisper-large-v3` model via the `AudioClient` to convert recorded audio streams into text with sub-second latency.
- **Session Continuity:** Using `AgentSession` to maintain conversation context across multiple voice turns, allowing the agent to remember previous parts of the spoken conversation.
- **Async Transcription Pattern:** Handling the transition from binary audio data to structured text using the `TranscribeAudioAsync` method from the OpenAI SDK.

### 32. ChatHistory.CustomPersistence (Automatic Session Rehydration)

This project demonstrates how to implement a custom persistence layer by extending the framework's `ChatHistoryProvider`. It automates the saving and loading of conversation history, allowing sessions to be resumed seamlessly using only a unique identifier.

**Key Concepts:**

- **Automated Lifecycle:** Unlike manual serialization, the `StoreChatHistoryAsync` method is triggered automatically by the agent during its execution loop, removing the need for boilerplate code after every `RunAsync` call.
- **Session Rehydration:** Demonstrates "rehydrating" an agent's memory by fetching stored JSON messages based on a `SessionId` retrieved from the `StateBag`.
- **State Management:** Utilizing the `InvokingContext.Session.StateBag` to persist and retrieve metadata (like IDs) across different execution turns.
- **Security through Isolation:** By storing only a GUID (Global Unique Identifier) on the client-side while keeping the actual conversation data in a secure store (like the local Temp folder or a database), you ensure that sensitive history is not exposed to the user directly.

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

### Raw Storage vs. Reduced Context

An important realization during development was that the `session.GetService<IList<ChatMessage>>()` call returns the raw, full history, while the agent sends the list trimmed by the Reducer to the Cerebras API in the background. This explains why more messages are visible in local memory than what the model actually "sees" at the moment of the call.

### The "Amnesia" Risk of Counting

When using `MessageCountingChatReducer`, if the `targetCount` is too low, the agent may fall into "amnesia". Since the counter is not content-aware, it might delete the user's introduction, leading to contradictory responses (e.g., the agent sees your name in its own previous answers but no longer knows where the information comes from, causing uncertainty).

### Summarization Performance on Cerebras

The `SummarizingChatReducer` requires an extra LLM call to generate the summary. Due to Cerebras' extreme low latency, this process is practically imperceptible to the end-user, while it can drastically reduce the number of input tokens (by up to 60-80%) in later stages of the conversation.

### The AG-UI Advantage: UI as a Tool

Project 22 highlights a paradigm shift: the UI is no longer just a display for the agent, but a set of tools the agent can manipulate.

- **Frontend Capabilities:** Traditionally, agents could only call backend APIs or local code. With AG-UI, an agent can decide to "Change the background to yellow" by invoking a tool that exists only on the user's machine.
- **Simplified Security:** Because the client only needs the server's URL, sensitive Cerebras or OpenAI API keys remain securely on the backend. This makes it ideal for public-facing AI applications.
- **Interactive Progress Monitoring:** Unlike raw SSE, AG-UI provides structured events for function calls and results, allowing the frontend to show "Thinking..." or "Calling Weather API..." indicators with minimal effort.

### The "RunAsync" Illusion in AG-UI

One of the most significant findings is that in the AG-UI protocol, the standard `RunAsync` method with a typed object result will not work as expected. The protocol will ignore the requested object format and return a standard text stream. To achieve structured output, you must:

1. Override `RunStreamingAsync` in a custom `AIAgent` class.
2. Manually capture function tool outputs.
3. `yield return` a `DataContent` object containing the JSON-serialized result.

### Client-Side Tool Execution

Traditional agents are limited to calling backend APIs. AG-UI turns the UI into a toolset. When an agent decides to "change the background to red," it isn't sending a command; it is executing a tool that exists only on the client's machine. This allows for highly interactive "AI-driven" interfaces where the agent has direct agency over the DOM.

### The TOON Advantage

During the implementation of Project 25, it became clear that TOON is a "game changer" for data-heavy tool calling:

- **The Verbosity Tax:** In standard JSON, repeating keys like "Name" or "Country" for 100+ records consumes thousands of unnecessary tokens.
- **The TOON Solution:** By stripping away quotes, colons, and repeating keys, TOON allows the agent to "see" more data within the same context window limit.
- **Accuracy Boost:** Surprisingly, models often show higher retrieval accuracy with TOON (approx. 74%) compared to JSON (approx. 70%), as the model isn't "distracted" by the repetitive structural noise of JSON.
- **Best Practice:** TOON should be used for flat lists and arrays. For deeply nested, complex objects, the overhead of converting to TOON might outweigh the benefits.

### The Power of Durable State

While Project 3 explored manual serialization to local files, Durable Agents automate this process at the infrastructure level.

- **Stateless vs. Stateful:** Standard HTTP-triggered functions are stateless. Durable Agents solve the "amnesia" problem by preserving the `AgentThread` in Azure Storage, allowing the agent to remember context across different requests.
- **Infrastructure Dependency:** Setting up Durable Agents requires a storage provider. During local development, the Azurite storage emulator (running in Docker) is mandatory.
- **The Startup Pitfall:** A common error during setup is the `QueueServiceClient` constructor exception. This typically occurs if the `AzureWebJobsStorage` connection string is missing from `local.settings.json` or if the storage emulator is not reachable.
- **Simplification:** Using `builder.ConfigureDurableAgents` significantly reduces boilerplate code for Dependency Injection and lifecycle management compared to manual implementations.

### The "Persistent Fact" vs. "Full History" Trade-off

Implementing a custom memory provider highlighted a major efficiency gain:

- **Token Optimization:** Instead of resending a massive, growing conversation history, we only send a concise list of extracted facts.
- **Asynchronous Processing:** Memory extraction happens in the background after the user has already received their response, ensuring zero impact on perceived latency.
- **Consistency:** By injecting facts directly into the system prompt, the agent maintains a consistent persona and knowledge base across entirely different sessions.

### The Challenge of Headerless WAVs

During the development of Project 29, a critical discovery was made regarding "OpenAI-compatible" TTS endpoints:

- **The Issue:** While the framework expects a standard file (like MP3 or a valid WAV with headers), some high-speed providers like Groq return raw PCM bytes. Attempting to use a standard `WaveFileReader` results in an `ArgumentOutOfRangeException` because the "RIFF" signature is missing.
- **The Solution:** By inspecting the byte length (verifying it's not a JSON error message) and wrapping the stream in a `RawSourceWaveStream` with a predefined `WaveFormat`, we achieve sub-second voice generation and playback without local file transcoding.

### The "Magic Parameter" (IServiceProvider) in Tools

Project 30 highlighted one of the framework's most useful "hidden" capabilities:

- **The Issue:** When a tool (e.g., database access or HTTP client) needs a service, but we register the tool as a static method or don't want to manually instantiate it with all its dependencies.
- **The Solution:** If we pass the `services: serviceProvider` parameter when creating the agent, the framework becomes capable of automatically populating the `IServiceProvider` parameter in the tool method's signature at the moment of the call.
- **Flexibility:** This allows tools to request necessary resources "on-the-fly" (`serviceProvider.GetRequiredService<T>()`), so not all dependencies need to be pre-manufactured during agent initialization.

### Tool Instantiation Strategies

During development, I identified three main patterns for registering tools:

- **Static (No-DI):** Simple helper functions, no need for external state.
- **Constructor DI:** The tool class receives its dependencies in the constructor (e.g., `ToolClass1(HttpClient)`). In this case, we must pass a ready, resolved instance to the agent.
- **Method DI (Service Injection):** The method itself requests the provider. This is the most dynamic method, especially useful for static tool methods that still need services.

### Manual Serialization vs. Provider-Based Persistence

In Project 3, we manually called `agent.Serialize()` and handled the file I/O in our main logic. Project 32 introduces a "Set and Forget" architecture:

- **The Advantage:** By injecting the `MyMessageStore` into the `ChatClientAgentOptions`, the agent handles its own memory management. This makes the code much cleaner and less error-prone.
- **Storage Flexibility:** While this implementation uses local JSON files for simplicity, the provider pattern is designed to scale to professional environments like SQL Server, Azure Blob Storage, or NoSQL databases without changing the agent's core logic.
- **Context Rehydration:** The model's "memory" is effectively restored because the provider injects the previous messages back into the prompt context before the LLM call is made, allowing for follow-up questions like "How tall is he?" to work perfectly even after a complete application restart.

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
