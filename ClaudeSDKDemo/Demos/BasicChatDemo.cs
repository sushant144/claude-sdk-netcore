using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace ClaudeSDKDemo.Demos;

public class BasicChatDemo(AnthropicClient anthropicClient, IConfiguration configuration)
{
    public async Task RunAsync()
    {
        Console.WriteLine("── Basic Chat Demo (IChatClient) ──");
        Console.WriteLine("Wraps AnthropicClient as IChatClient via .AsChatClient()");
        Console.WriteLine();

        var model = configuration["Anthropic:Model"] ?? "claude-sonnet-4-6";
        IChatClient chatClient = anthropicClient.AsChatClient(model);

        // Single-turn
        Console.WriteLine("Prompt: Explain .NET and AI integration in one paragraph.");
        Console.WriteLine();

        var response = await chatClient.GetResponseAsync(
            "Explain .NET and AI integration in one paragraph.");

        Console.WriteLine("Response:");
        Console.WriteLine(response.Text);
        Console.WriteLine();

        // Multi-turn conversation
        Console.WriteLine("── Multi-turn conversation ──");
        var history = new List<ChatMessage>
        {
            new(ChatRole.User, "What is the Anthropic C# SDK?")
        };

        var firstReply = await chatClient.GetResponseAsync(history);
        Console.WriteLine($"User: What is the Anthropic C# SDK?");
        Console.WriteLine($"Claude: {firstReply.Text}");
        Console.WriteLine();

        history.Add(new ChatMessage(ChatRole.Assistant, firstReply.Text));
        history.Add(new ChatMessage(ChatRole.User, "How does it compare to OpenAI's SDK?"));

        var secondReply = await chatClient.GetResponseAsync(history);
        Console.WriteLine($"User: How does it compare to OpenAI's SDK?");
        Console.WriteLine($"Claude: {secondReply.Text}");
    }
}
