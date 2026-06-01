using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace ClaudeSDKDemo.Demos;

public class StreamingDemo(AnthropicClient anthropicClient, IConfiguration configuration)
{
    public async Task RunAsync()
    {
        Console.WriteLine("── Streaming Response Demo ──");
        Console.WriteLine("Tokens arrive and print in real-time via GetStreamingResponseAsync.");
        Console.WriteLine();

        var model = configuration["Anthropic:Model"] ?? "claude-sonnet-4-6";
        IChatClient chatClient = anthropicClient.AsChatClient(model);

        const string prompt = "Write a short poem about .NET developers building AI applications.";
        Console.WriteLine($"Prompt: {prompt}");
        Console.WriteLine();
        Console.WriteLine("Streaming response:");
        Console.WriteLine();

        await foreach (var update in chatClient.GetStreamingResponseAsync(prompt))
        {
            Console.Write(update.Text);
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("── Stream complete ──");
    }
}
