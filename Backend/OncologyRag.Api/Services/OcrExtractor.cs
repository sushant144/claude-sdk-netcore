using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace OncologyRag.Api.Services;

public class OcrExtractor(ILogger<OcrExtractor> logger)
{
    private const int MinTextLengthThreshold = 50;

    public List<(int PageNumber, string Text)> Extract(string pdfPath)
    {
        var results = new List<(int PageNumber, string Text)>();

        try
        {
            using var document = PdfDocument.Open(pdfPath);
            foreach (var page in document.GetPages())
            {
                var text = ExtractPageText(page);
                if (!string.IsNullOrWhiteSpace(text))
                    results.Add((page.Number, text));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract text from PDF: {Path}", pdfPath);
        }

        return results;
    }

    private static string ExtractPageText(Page page)
    {
        var sb = new StringBuilder();
        foreach (var word in page.GetWords())
        {
            sb.Append(word.Text);
            sb.Append(' ');
        }
        var text = sb.ToString().Trim();

        // If text is too short, the page may be scanned/image-only
        if (text.Length < MinTextLengthThreshold)
            return string.Empty;

        return text;
    }

    public int GetPageCount(string pdfPath)
    {
        try
        {
            using var document = PdfDocument.Open(pdfPath);
            return document.NumberOfPages;
        }
        catch
        {
            return 0;
        }
    }
}
