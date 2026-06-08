using OncologyRag.Api.Models;

namespace OncologyRag.Api.Services;

public class IndexingService(
    PdfDownloader downloader,
    OcrExtractor extractor,
    TextChunker chunker,
    EmbeddingService embedder,
    VectorStoreService vectorStore,
    IConfiguration configuration,
    ILogger<IndexingService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        // Run indexing in background so the API starts immediately
        _ = Task.Run(() => IndexAllAsync(ct), ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    public async Task IndexAllAsync(CancellationToken ct)
    {
        try
        {
            await vectorStore.EnsureSchemaAsync(ct);

            var existing = await vectorStore.GetChunkCountAsync(ct);
            if (existing > 0)
            {
                logger.LogInformation("Skipping indexing — {Count} chunks already indexed.", existing);
                return;
            }

            var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
                ?? configuration["Anthropic:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                logger.LogWarning("ANTHROPIC_API_KEY not set — skipping PDF indexing.");
                return;
            }

            var sources = configuration.GetSection("PdfSources").Get<List<PdfSource>>() ?? [];

            foreach (var source in sources)
            {
                logger.LogInformation("Processing: {Name}", source.Name);

                var localPath = await downloader.DownloadAsync(source, ct);
                if (localPath is null) continue;

                var pages = extractor.Extract(localPath);
                var pageCount = extractor.GetPageCount(localPath);
                logger.LogInformation("Extracted {Pages} pages from {Name}", pages.Count, source.Name);

                var chunks = chunker.Chunk(source.Name, source.Url, pages);
                logger.LogInformation("Created {Chunks} chunks from {Name}", chunks.Count, source.Name);

                var docId = await vectorStore.UpsertDocumentAsync(source.Name, source.Url, pageCount, ct);

                var embedded = 0;
                foreach (var chunk in chunks)
                {
                    var embedding = await embedder.EmbedAsync(chunk.Content, apiKey, ct);
                    await vectorStore.UpsertChunkAsync(docId, chunk, embedding, ct);
                    embedded++;
                    if (embedded % 10 == 0)
                        logger.LogInformation("  Embedded {N}/{Total} chunks from {Name}", embedded, chunks.Count, source.Name);
                }

                await vectorStore.UpdateChunkCountAsync(docId, chunks.Count, ct);
                logger.LogInformation("Indexed {Name}: {Chunks} chunks", source.Name, chunks.Count);
            }

            logger.LogInformation("Indexing complete.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Indexing failed.");
        }
    }
}
