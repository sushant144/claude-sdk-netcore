# Oncology RAG — Claude AI + pgvector + Angular

A full-stack proof-of-concept demonstrating Retrieval-Augmented Generation (RAG) in the oncology domain using the Anthropic C# SDK, pgvector, and an Angular frontend.

## Architecture

```
Angular UI (port 4200)
    │  SSE stream / REST
    ▼
.NET 8 Web API (port 5000)
    ├── POST /api/auth/validate    — key validation (never stored)
    ├── GET  /api/documents        — indexed document metadata
    └── POST /api/chat             — RAG pipeline + SSE streaming
           │
           ├── EmbeddingService   — Voyage AI embeddings (via Anthropic key)
           ├── VectorStoreService — pgvector similarity search
           └── AnthropicClient    — Claude streaming response
                │
                ▼
          pgvector (PostgreSQL)
```

**RAG Pipeline:** NCI oncology PDFs → PdfPig OCR → sliding-window chunking → Voyage embeddings → pgvector upsert. On each query the user's question is embedded, top-K chunks are retrieved, and if the best cosine similarity score exceeds the threshold the chunks are injected into the Claude prompt as context.

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and Angular CLI (`npm install -g @angular/cli`)
- Docker (for pgvector)
- Anthropic API key

## Setup

### 1. Start pgvector

```bash
docker run -d --name pgvector \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  pgvector/pgvector:pg16

psql -U postgres -c "CREATE DATABASE oncology_rag;"
psql -U postgres -d oncology_rag -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

### 2. Backend

```bash
cd Backend/OncologyRag.Api
export ANTHROPIC_API_KEY=sk-ant-...
dotnet run
```

On first start the API automatically downloads and indexes the configured NCI PDF sources. Progress is logged to the console.

```bash
# Run tests
dotnet test Backend/OncologyRag.Tests/OncologyRag.Tests.csproj
```

### 3. Frontend

```bash
cd Frontend/oncology-rag-ui
npm install
ng serve
```

Open **http://localhost:4200** — you'll be prompted to enter your Anthropic API key (held in memory only, never stored).

## Configuration

Edit `Backend/OncologyRag.Api/appsettings.json`:

| Key | Default | Description |
|---|---|---|
| `Anthropic:Model` | `claude-sonnet-4-6` | Claude model for generation |
| `ConnectionStrings:Postgres` | localhost:5432 | pgvector connection |
| `PdfSources` | NCI PDFs | List of `{ Url, Name }` PDF sources to index |
| `Rag:ChunkSize` | 500 | Words per chunk |
| `Rag:ChunkOverlap` | 50 | Overlap between consecutive chunks |
| `Rag:TopK` | 5 | Number of chunks to retrieve per query |
| `Rag:SimilarityThreshold` | 0.65 | Minimum cosine similarity to answer (below = out-of-scope) |

## Features

- **PDF ingestion** — Downloads NCI oncology PDFs, extracts text with PdfPig
- **Chunking** — Sliding window with configurable size and overlap
- **Vector embeddings** — Voyage AI (1024-dim) via Anthropic API key
- **pgvector similarity search** — IVFFlat index, cosine distance
- **Bounds checking** — Queries below the similarity threshold are rejected with an explanation rather than hallucinating an answer
- **Streaming** — Claude's response streams token-by-token via Server-Sent Events
- **Token usage** — Input/output/total token counts shown after each response
- **API key UX** — Password input with live validation; key held in Angular memory only, never persisted
