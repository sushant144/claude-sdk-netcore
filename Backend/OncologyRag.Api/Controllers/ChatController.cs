using System.Text;
using System.Text.Json;
using Anthropic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using OncologyRag.Api.Services;

namespace OncologyRag.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController(
    EmbeddingService embedder,
    VectorStoreService vectorStore,
    IConfiguration configuration,
    ILogger<ChatController> logger) : ControllerBase
{
    private int TopK => configuration.GetValue("Rag:TopK", 5);
    private float Threshold => configuration.GetValue("Rag:SimilarityThreshold", 0.65f);

    [HttpPost]
    public async Task StreamChat([FromBody] Models.ChatRequest request, CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        async Task SendEventAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var line = $"data: {json}\n\n";
            await Response.WriteAsync(line, ct);
            await Response.Body.FlushAsync(ct);
        }

        try
        {
            // Embed the user's question
            var queryEmbedding = await embedder.EmbedAsync(request.Message, request.ApiKey, ct);

            // Search vector store
            var results = await vectorStore.SearchAsync(queryEmbedding, TopK, ct);
            var bestScore = results.Count > 0 ? results[0].Score : 0f;

            // Bounds check
            if (bestScore < Threshold)
            {
                await SendEventAsync(new { type = "bounds_check", inScope = false, reason = "This question appears to be outside the scope of the indexed oncology documents. Please ask something related to the available cancer guidelines." });
                await SendEventAsync(new { type = "done" });
                return;
            }

            await SendEventAsync(new { type = "bounds_check", inScope = true });

            // Send sources
            var sources = results.Select(r => new { file = r.SourceName, url = r.SourceUrl, page = r.PageNumber, score = Math.Round(r.Score, 3) });
            await SendEventAsync(new { type = "sources", items = sources });

            // Build augmented prompt
            var contextBuilder = new StringBuilder();
            foreach (var r in results)
                contextBuilder.AppendLine($"[Source: {r.SourceName}, Page {r.PageNumber}]\n{r.Content}\n");

            var augmentedPrompt = $"""
                You are a clinical oncology assistant. Answer the question below based ONLY on the provided context.
                If the answer cannot be found in the context, say so explicitly. Do not fabricate information.

                CONTEXT:
                {contextBuilder}

                QUESTION: {request.Message}
                """;

            // Stream Claude response
            var anthropicClient = new AnthropicClient(request.ApiKey);
            IChatClient chatClient = anthropicClient.AsChatClient(configuration["Anthropic:Model"] ?? "claude-sonnet-4-6");

            int inputTokens = 0, outputTokens = 0;

            await foreach (var update in chatClient.GetStreamingResponseAsync(augmentedPrompt, cancellationToken: ct))
            {
                if (!string.IsNullOrEmpty(update.Text))
                    await SendEventAsync(new { type = "token", text = update.Text });

                // Capture usage if present
                if (update.Usage is { } usage)
                {
                    inputTokens += usage.InputTokenCount ?? 0;
                    outputTokens += usage.OutputTokenCount ?? 0;
                }
            }

            await SendEventAsync(new { type = "usage", inputTokens, outputTokens });
            await SendEventAsync(new { type = "done" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Chat stream error");
            await SendEventAsync(new { type = "error", message = "An error occurred while processing your request." });
            await SendEventAsync(new { type = "done" });
        }
    }
}
