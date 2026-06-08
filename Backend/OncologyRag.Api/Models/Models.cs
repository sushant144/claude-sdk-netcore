namespace OncologyRag.Api.Models;

public record DocumentDto(
    int Id,
    string FileName,
    string Url,
    int PageCount,
    int ChunkCount,
    DateTime IndexedAt);

public record ValidateKeyRequest(string ApiKey);
public record ValidateKeyResponse(bool Valid, string? Error = null);

public record ChatRequest(string Message, string ApiKey);

public record PdfSource(string Url, string Name);

public record TextChunk(
    string SourceName,
    string SourceUrl,
    int PageNumber,
    int ChunkIndex,
    string Content);

public record SearchResult(
    string SourceName,
    string SourceUrl,
    int PageNumber,
    int ChunkIndex,
    string Content,
    float Score);
