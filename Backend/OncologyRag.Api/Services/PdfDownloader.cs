using OncologyRag.Api.Models;

namespace OncologyRag.Api.Services;

public class PdfDownloader(IHttpClientFactory httpClientFactory, ILogger<PdfDownloader> logger)
{
    private readonly string _downloadDir = Path.Combine(Path.GetTempPath(), "oncology_pdfs");

    public async Task<string?> DownloadAsync(PdfSource source, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_downloadDir);

        var safeName = string.Join("_", source.Name.Split(Path.GetInvalidFileNameChars()));
        var localPath = Path.Combine(_downloadDir, $"{safeName}.pdf");

        if (File.Exists(localPath))
        {
            logger.LogInformation("PDF already downloaded: {Name}", source.Name);
            return localPath;
        }

        try
        {
            var client = httpClientFactory.CreateClient("pdf");
            var bytes = await client.GetByteArrayAsync(source.Url, ct);
            await File.WriteAllBytesAsync(localPath, bytes, ct);
            logger.LogInformation("Downloaded PDF: {Name} ({Size} bytes)", source.Name, bytes.Length);
            return localPath;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download PDF: {Url}", source.Url);
            return null;
        }
    }
}
