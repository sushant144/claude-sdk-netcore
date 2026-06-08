using OncologyRag.Api.Models;

namespace OncologyRag.Api.Services;

public class TextChunker(IConfiguration configuration)
{
    private int ChunkSize => configuration.GetValue("Rag:ChunkSize", 500);
    private int ChunkOverlap => configuration.GetValue("Rag:ChunkOverlap", 50);

    public List<TextChunk> Chunk(string sourceName, string sourceUrl, List<(int PageNumber, string Text)> pages)
    {
        var chunks = new List<TextChunk>();

        foreach (var (pageNumber, text) in pages)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var chunkIndex = 0;
            var start = 0;

            while (start < words.Length)
            {
                var end = Math.Min(start + ChunkSize, words.Length);
                var chunkText = string.Join(" ", words[start..end]);

                chunks.Add(new TextChunk(sourceName, sourceUrl, pageNumber, chunkIndex++, chunkText));

                start += ChunkSize - ChunkOverlap;
                if (start >= words.Length) break;
            }
        }

        return chunks;
    }
}
