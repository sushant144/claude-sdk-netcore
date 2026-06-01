using Anthropic;
using ClaudeSDKDemo.Demos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? configuration["Anthropic:ApiKey"]
    ?? throw new InvalidOperationException("ANTHROPIC_API_KEY is not set.");

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton(new AnthropicClient(apiKey));
services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
services.AddTransient<BasicChatDemo>();
services.AddTransient<StreamingDemo>();
services.AddTransient<MultiCloudDemo>();

var provider = services.BuildServiceProvider();

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║   Anthropic C# SDK for Claude AI — POC   ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();

while (true)
{
    Console.WriteLine("Select a demo:");
    Console.WriteLine("  [1] Basic Chat (IChatClient)");
    Console.WriteLine("  [2] Streaming Response");
    Console.WriteLine("  [3] Multi-Cloud Provider Config");
    Console.WriteLine("  [Q] Quit");
    Console.Write("\nChoice: ");

    var input = Console.ReadLine()?.Trim().ToUpperInvariant();
    Console.WriteLine();

    switch (input)
    {
        case "1":
            await provider.GetRequiredService<BasicChatDemo>().RunAsync();
            break;
        case "2":
            await provider.GetRequiredService<StreamingDemo>().RunAsync();
            break;
        case "3":
            await provider.GetRequiredService<MultiCloudDemo>().RunAsync();
            break;
        case "Q":
            Console.WriteLine("Goodbye!");
            return;
        default:
            Console.WriteLine("Invalid choice, try again.\n");
            break;
    }

    Console.WriteLine();
}
