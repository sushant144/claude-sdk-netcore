using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using OncologyRag.Api.Models;
using OncologyRag.Api.Services;
using Xunit;

namespace OncologyRag.Tests.Services;

public class RagPipelineTests
{
    [Fact]
    public void TextChunker_MultiplePages_ChunksEachPage()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rag:ChunkSize"] = "5",
                ["Rag:ChunkOverlap"] = "1"
            })
            .Build();
        var chunker = new TextChunker(config);

        var pages = new List<(int, string)>
        {
            (1, "word1 word2 word3 word4 word5 word6 word7"),
            (2, "pageTwo word1 word2 word3 word4 word5 word6")
        };

        var chunks = chunker.Chunk("TestDoc", "http://test", pages);

        Assert.Contains(chunks, c => c.PageNumber == 1);
        Assert.Contains(chunks, c => c.PageNumber == 2);
    }

    [Fact]
    public void TextChunk_ContentIsNonEmpty()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rag:ChunkSize"] = "10",
                ["Rag:ChunkOverlap"] = "2"
            })
            .Build();
        var chunker = new TextChunker(config);

        var text = string.Join(" ", Enumerable.Range(1, 25).Select(i => $"token{i}"));
        var pages = new List<(int, string)> { (1, text) };
        var chunks = chunker.Chunk("doc", "http://test", pages);

        Assert.All(chunks, c => Assert.False(string.IsNullOrWhiteSpace(c.Content)));
    }

    [Fact]
    public void OcrExtractor_NonExistentFile_ReturnsEmpty()
    {
        var extractor = new OcrExtractor(NullLogger<OcrExtractor>.Instance);
        var result = extractor.Extract("/tmp/nonexistent_file_xyz.pdf");
        Assert.Empty(result);
    }

    [Fact]
    public void OcrExtractor_GetPageCount_NonExistentFile_ReturnsZero()
    {
        var extractor = new OcrExtractor(NullLogger<OcrExtractor>.Instance);
        var count = extractor.GetPageCount("/tmp/nonexistent_file_xyz.pdf");
        Assert.Equal(0, count);
    }
}
