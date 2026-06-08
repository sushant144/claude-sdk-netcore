using Microsoft.Extensions.Configuration;
using OncologyRag.Api.Services;
using Xunit;

namespace OncologyRag.Tests.Services;

public class TextChunkerTests
{
    private static TextChunker CreateChunker(int chunkSize = 10, int overlap = 2)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rag:ChunkSize"] = chunkSize.ToString(),
                ["Rag:ChunkOverlap"] = overlap.ToString()
            })
            .Build();
        return new TextChunker(config);
    }

    [Fact]
    public void Chunk_EmptyText_ReturnsNoChunks()
    {
        var chunker = CreateChunker();
        var result = chunker.Chunk("doc", "http://test", []);
        Assert.Empty(result);
    }

    [Fact]
    public void Chunk_ShortText_ReturnsSingleChunk()
    {
        var chunker = CreateChunker(chunkSize: 100, overlap: 10);
        var pages = new List<(int, string)> { (1, "This is a short text with only a few words.") };
        var result = chunker.Chunk("doc", "http://test", pages);
        Assert.Single(result);
    }

    [Fact]
    public void Chunk_LongText_ReturnsMultipleChunks()
    {
        var chunker = CreateChunker(chunkSize: 5, overlap: 1);
        var words = string.Join(" ", Enumerable.Range(1, 20).Select(i => $"word{i}"));
        var pages = new List<(int, string)> { (1, words) };
        var result = chunker.Chunk("doc", "http://test", pages);
        Assert.True(result.Count > 1);
    }

    [Fact]
    public void Chunk_SetsSourceNameAndUrl()
    {
        var chunker = CreateChunker();
        var pages = new List<(int, string)> { (1, string.Join(" ", Enumerable.Range(1, 5).Select(i => $"word{i}"))) };
        var result = chunker.Chunk("MyDoc", "http://example.com", pages);
        Assert.All(result, c =>
        {
            Assert.Equal("MyDoc", c.SourceName);
            Assert.Equal("http://example.com", c.SourceUrl);
        });
    }

    [Fact]
    public void Chunk_IncrementingChunkIndex()
    {
        var chunker = CreateChunker(chunkSize: 3, overlap: 1);
        var words = string.Join(" ", Enumerable.Range(1, 15).Select(i => $"word{i}"));
        var pages = new List<(int, string)> { (1, words) };
        var result = chunker.Chunk("doc", "http://test", pages);
        for (var i = 0; i < result.Count; i++)
            Assert.Equal(i, result[i].ChunkIndex);
    }

    [Fact]
    public void Chunk_OverlapProducesSharedWords()
    {
        var chunker = CreateChunker(chunkSize: 4, overlap: 2);
        var words = "a b c d e f g h";
        var pages = new List<(int, string)> { (1, words) };
        var result = chunker.Chunk("doc", "http://test", pages);

        // Second chunk should start with words from end of first chunk
        var firstChunkWords = result[0].Content.Split(' ');
        var secondChunkWords = result[1].Content.Split(' ');
        var overlapWords = firstChunkWords.TakeLast(2).ToArray();
        Assert.Equal(overlapWords[0], secondChunkWords[0]);
    }
}
