using Anthropic;
using Microsoft.Extensions.Configuration;

namespace ClaudeSDKDemo.Demos;

public class MultiCloudDemo(IConfiguration configuration)
{
    public Task RunAsync()
    {
        Console.WriteLine("── Multi-Cloud Provider Config Demo ──");
        Console.WriteLine("The Anthropic C# SDK supports routing to multiple cloud providers.");
        Console.WriteLine("Below are the configuration patterns for each — no live calls are made");
        Console.WriteLine("since each provider requires its own credentials.");
        Console.WriteLine();

        ShowDirectAnthropicConfig();
        ShowAwsBedrockConfig();
        ShowAzureConfig();
        ShowVertexAiConfig();

        Console.WriteLine("── How routing works ──");
        Console.WriteLine("Set the 'Provider' key in appsettings.json (or env var) to switch providers.");
        Console.WriteLine($"Current configured provider: {configuration["Provider"] ?? "Anthropic"}");

        return Task.CompletedTask;
    }

    private static void ShowDirectAnthropicConfig()
    {
        Console.WriteLine("1. Direct Anthropic API");
        Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("   │ var client = new AnthropicClient(apiKey);               │");
        Console.WriteLine("   │ IChatClient chat = client.AsChatClient(model);          │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────┘");
        Console.WriteLine();
    }

    private static void ShowAwsBedrockConfig()
    {
        Console.WriteLine("2. AWS Bedrock");
        Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("   │ var client = new AnthropicClient(new AnthropicClientOptions│");
        Console.WriteLine("   │ {                                                        │");
        Console.WriteLine("   │   BaseUrl = new Uri(                                    │");
        Console.WriteLine("   │     \"https://bedrock-runtime.us-east-1.amazonaws.com\"), │");
        Console.WriteLine("   │   AuthToken = \"<AWS_SESSION_TOKEN>\"                     │");
        Console.WriteLine("   │ });                                                      │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────┘");
        Console.WriteLine("   Model ID: anthropic.claude-sonnet-4-6");
        Console.WriteLine();
    }

    private static void ShowAzureConfig()
    {
        Console.WriteLine("3. Azure AI (via Azure Marketplace / AI Foundry)");
        Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("   │ var client = new AnthropicClient(new AnthropicClientOptions│");
        Console.WriteLine("   │ {                                                        │");
        Console.WriteLine("   │   BaseUrl = new Uri(                                    │");
        Console.WriteLine("   │     \"https://<resource>.openai.azure.com/\"),            │");
        Console.WriteLine("   │   AuthToken = \"<AZURE_API_KEY>\"                         │");
        Console.WriteLine("   │ });                                                      │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────┘");
        Console.WriteLine();
    }

    private static void ShowVertexAiConfig()
    {
        Console.WriteLine("4. Google Cloud Vertex AI");
        Console.WriteLine("   ┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("   │ var client = new AnthropicClient(new AnthropicClientOptions│");
        Console.WriteLine("   │ {                                                        │");
        Console.WriteLine("   │   BaseUrl = new Uri(                                    │");
        Console.WriteLine("   │     \"https://<region>-aiplatform.googleapis.com/\"),     │");
        Console.WriteLine("   │   AuthToken = \"<GCP_ACCESS_TOKEN>\"                      │");
        Console.WriteLine("   │ });                                                      │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────┘");
        Console.WriteLine();
    }
}
