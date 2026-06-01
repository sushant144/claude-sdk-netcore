# Anthropic C# SDK for Claude AI — .NET 8 POC

A proof-of-concept console app demonstrating the [Anthropic official C# SDK](https://github.com/anthropics/anthropic-sdk-dotnet) for Claude AI on .NET 8.

## Features

- **Basic Chat** — `IChatClient` integration via `Microsoft.Extensions.AI`, single-turn and multi-turn conversations
- **Streaming** — Real-time token-by-token responses using `GetStreamingResponseAsync`
- **Multi-Cloud Routing** — Configuration patterns for AWS Bedrock, Azure AI, and Google Vertex AI
- **Dependency Injection** — Clean DI setup with `IConfiguration`, `AnthropicClient`, and `ILogger`

## Quick Start

```bash
# 1. Set your API key
export ANTHROPIC_API_KEY=sk-ant-...

# 2. Run
dotnet run --project ClaudeSDKDemo/ClaudeSDKDemo.csproj
```

## Project Structure

```
ClaudeSDKDemo/
├── Program.cs                  # DI setup + interactive menu
├── appsettings.json            # Model and provider config
└── Demos/
    ├── BasicChatDemo.cs        # IChatClient usage
    ├── StreamingDemo.cs        # Token-by-token streaming
    └── MultiCloudDemo.cs       # Azure / Bedrock / Vertex AI config
```

## Configuration

`appsettings.json` sets defaults. The API key is resolved in this order:

1. `ANTHROPIC_API_KEY` environment variable
2. `Anthropic:ApiKey` in `appsettings.json`

To change the model, update `Anthropic:Model` in `appsettings.json` (default: `claude-sonnet-4-6`).

## Install

```bash
dotnet add package Anthropic
dotnet add package Microsoft.Extensions.AI.Anthropic
```
