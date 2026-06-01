# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

A .NET 8 console POC demonstrating the Anthropic official C# SDK for Claude AI. It showcases IChatClient integration, streaming, and multi-cloud provider configuration.

## Commands

```bash
# Build
dotnet build ClaudeSDKDemo/ClaudeSDKDemo.csproj

# Run
export ANTHROPIC_API_KEY=sk-ant-...
dotnet run --project ClaudeSDKDemo/ClaudeSDKDemo.csproj

# Restore packages
dotnet restore ClaudeSDKDemo/ClaudeSDKDemo.csproj
```

## Architecture

`ClaudeSDKDemo/Program.cs` sets up DI (IConfiguration, AnthropicClient, ILogger) and presents an interactive menu that delegates to demo classes.

Each demo lives in `ClaudeSDKDemo/Demos/` and receives an injected `AnthropicClient` and `IConfiguration`:

- **BasicChatDemo** — wraps `AnthropicClient` as `IChatClient` (via `.AsChatClient()`) and calls `GetResponseAsync`
- **StreamingDemo** — uses `GetStreamingResponseAsync` to print tokens as they arrive
- **MultiCloudDemo** — shows config patterns for AWS Bedrock, Azure, and Vertex AI routing; does not make live calls (credentials differ per provider)

API key is read from env var `ANTHROPIC_API_KEY` first, falling back to `appsettings.json`. Model defaults to `claude-sonnet-4-6` and can be overridden in config.
