# Microsoft Agent Framework Learning Sandbox

This repository documents my journey exploring the **Microsoft Agent Framework**. It contains a collection of isolated projects, each focusing on different capabilities, integrations, and patterns within the ecosystem.

> **Acknowledgement:** This repository is inspired by and adapted from [rwjdk/MicrosoftAgentFrameworkSamples](https://github.com/rwjdk/MicrosoftAgentFrameworkSamples). The samples here are rewritten to utilize **Google Gemini** models.

## Projects

### 1. ZeroToFirstAgent.GoogleGemini

This is the "Hello World" of the repository. A simple Console Application that demonstrates how to connect the Microsoft Agent Framework with Google's Gemini LLM.

**Key Concepts:**
- Setting up `IChatClient` with `Google_GenerativeAI.Microsoft`.
- Creating a basic `ChatClientAgent`.
- Streaming vs. Non-streaming responses.

---

## Getting Started

### Prerequisites
1. **.NET SDK** (version 10)
2. A **Google Gemini API Key** (Get it from Google AI Studio)

### Configuration Pattern

Most projects in this repository use a **layered configuration** approach to keep API keys secure.

1. **Clone the repository.**
2. **Navigate to the specific project folder** (e.g., `ZeroToFirstAgent.GoogleGemini`).
3. **Create a `appsettings.Local.json` file** in the project root:
   ```json
   {
     "GenerativeAI": {
       "ApiKey": "YOUR_ACTUAL_GOOGLE_API_KEY_HERE"
     }
   }
   ```
   *Note: This file is ignored by git.*
4. **Run the project:** `dotnet run`
