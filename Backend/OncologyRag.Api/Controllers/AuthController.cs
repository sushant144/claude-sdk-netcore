using Anthropic;
using Microsoft.AspNetCore.Mvc;
using OncologyRag.Api.Models;

namespace OncologyRag.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("validate")]
    public async Task<ActionResult<ValidateKeyResponse>> ValidateKey([FromBody] ValidateKeyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ApiKey))
            return BadRequest(new ValidateKeyResponse(false, "API key is required."));

        try
        {
            // Create a throwaway client — key is never stored
            var client = new AnthropicClient(request.ApiKey);
            await client.Messages.CreateAsync(new()
            {
                Model = "claude-haiku-4-5-20251001",
                MaxTokens = 1,
                Messages = [new() { Role = MessageRole.User, Content = "Hi" }]
            });

            return Ok(new ValidateKeyResponse(true));
        }
        catch (Exception ex)
        {
            logger.LogWarning("API key validation failed: {Message}", ex.Message);
            return BadRequest(new ValidateKeyResponse(false, "Invalid API key. Please check and try again."));
        }
    }
}
