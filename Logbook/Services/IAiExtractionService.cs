namespace Logbook.Services;

/// <summary>
/// Defines AI-assisted extraction of job listing fields from URL or text input.
/// </summary>
public interface IAiExtractionService
{
    /// <summary>
    /// Accepts a job listing URL or raw text. Fetches page content if a URL is provided,
    /// falling back to the raw input if the fetch fails. Submits content to the
    /// Anthropic Claude Haiku API and returns extracted field values.
    /// </summary>
    Task<ExtractionResult> ExtractAsync(string input);
}
/// <summary>
/// Represents the result of an AI extraction attempt.
/// </summary>
/// <param name="CompanyName">Extracted company name, or null if not found.</param>
/// <param name="RoleTitle">Extracted job title, or null if not found.</param>
/// <param name="Source">Detected job board or platform, or null if not found.</param>
/// <param name="Notes">AI-generated summary of key requirements.</param>
/// <param name="Success">True if extraction completed without error.</param>
/// <param name="ErrorMessage">Error description if Success is false, otherwise null.</param>
public record ExtractionResult(
    string? CompanyName,
    string? RoleTitle,
    string? Source,
    string? Notes,
    bool Success,
    string? ErrorMessage
);