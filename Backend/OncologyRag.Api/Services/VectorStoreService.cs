using Npgsql;
using OncologyRag.Api.Models;
using Pgvector;

namespace OncologyRag.Api.Services;

public class VectorStoreService(IConfiguration configuration, ILogger<VectorStoreService> logger)
{
    private string ConnectionString => configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("Postgres connection string not configured.");

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE EXTENSION IF NOT EXISTS vector;

            CREATE TABLE IF NOT EXISTS oncology_documents (
                id SERIAL PRIMARY KEY,
                file_name TEXT NOT NULL,
                source_url TEXT NOT NULL,
                page_count INT NOT NULL DEFAULT 0,
                chunk_count INT NOT NULL DEFAULT 0,
                indexed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(file_name)
            );

            CREATE TABLE IF NOT EXISTS oncology_chunks (
                id SERIAL PRIMARY KEY,
                document_id INT NOT NULL REFERENCES oncology_documents(id),
                source_name TEXT NOT NULL,
                source_url TEXT NOT NULL,
                page_number INT NOT NULL,
                chunk_index INT NOT NULL,
                content TEXT NOT NULL,
                embedding vector(1024),
                UNIQUE(source_name, chunk_index)
            );

            CREATE INDEX IF NOT EXISTS idx_oncology_chunks_embedding
                ON oncology_chunks USING ivfflat (embedding vector_cosine_ops)
                WITH (lists = 10);
            """;
        await cmd.ExecuteNonQueryAsync(ct);
        logger.LogInformation("Database schema verified.");
    }

    public async Task<int> GetChunkCountAsync(CancellationToken ct = default)
    {
        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM oncology_chunks";
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    public async Task<int> UpsertDocumentAsync(string fileName, string sourceUrl, int pageCount, CancellationToken ct = default)
    {
        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO oncology_documents (file_name, source_url, page_count, indexed_at)
            VALUES (@fileName, @sourceUrl, @pageCount, NOW())
            ON CONFLICT (file_name) DO UPDATE SET page_count = @pageCount, indexed_at = NOW()
            RETURNING id
            """;
        cmd.Parameters.AddWithValue("fileName", fileName);
        cmd.Parameters.AddWithValue("sourceUrl", sourceUrl);
        cmd.Parameters.AddWithValue("pageCount", pageCount);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
    }

    public async Task UpdateChunkCountAsync(int documentId, int chunkCount, CancellationToken ct = default)
    {
        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE oncology_documents SET chunk_count = @count WHERE id = @id";
        cmd.Parameters.AddWithValue("count", chunkCount);
        cmd.Parameters.AddWithValue("id", documentId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpsertChunkAsync(int documentId, TextChunk chunk, float[] embedding, CancellationToken ct = default)
    {
        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO oncology_chunks (document_id, source_name, source_url, page_number, chunk_index, content, embedding)
            VALUES (@docId, @sourceName, @sourceUrl, @pageNum, @chunkIdx, @content, @embedding)
            ON CONFLICT (source_name, chunk_index) DO NOTHING
            """;
        cmd.Parameters.AddWithValue("docId", documentId);
        cmd.Parameters.AddWithValue("sourceName", chunk.SourceName);
        cmd.Parameters.AddWithValue("sourceUrl", chunk.SourceUrl);
        cmd.Parameters.AddWithValue("pageNum", chunk.PageNumber);
        cmd.Parameters.AddWithValue("chunkIdx", chunk.ChunkIndex);
        cmd.Parameters.AddWithValue("content", chunk.Content);
        cmd.Parameters.Add(new NpgsqlParameter("embedding", new Vector(embedding)));
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<List<SearchResult>> SearchAsync(float[] queryEmbedding, int topK, CancellationToken ct = default)
    {
        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT source_name, source_url, page_number, chunk_index, content,
                   1 - (embedding <=> @embedding) AS score
            FROM oncology_chunks
            ORDER BY embedding <=> @embedding
            LIMIT @topK
            """;
        cmd.Parameters.Add(new NpgsqlParameter("embedding", new Vector(queryEmbedding)));
        cmd.Parameters.AddWithValue("topK", topK);

        var results = new List<SearchResult>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(new SearchResult(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetString(4),
                reader.GetFloat(5)));
        }
        return results;
    }

    public async Task<List<DocumentDto>> GetDocumentsAsync(CancellationToken ct = default)
    {
        await using var conn = await OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, file_name, source_url, page_count, chunk_count, indexed_at FROM oncology_documents ORDER BY indexed_at DESC";

        var docs = new List<DocumentDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            docs.Add(new DocumentDto(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetDateTime(5)));
        }
        return docs;
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken ct)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();
        return await dataSource.OpenConnectionAsync(ct);
    }
}
