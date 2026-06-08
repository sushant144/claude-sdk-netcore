using Microsoft.AspNetCore.Mvc;
using OncologyRag.Api.Models;
using OncologyRag.Api.Services;

namespace OncologyRag.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController(VectorStoreService vectorStore) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DocumentDto>>> GetDocuments(CancellationToken ct)
    {
        var docs = await vectorStore.GetDocumentsAsync(ct);
        return Ok(docs);
    }
}
