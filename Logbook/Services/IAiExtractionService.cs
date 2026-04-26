namespace Logbook.Services;

public interface IAiExtractionService
{
    Task<ExtractionResult> ExtractAsync(string input);
}

public record ExtractionResult(
    string? CompanyName,
    string? RoleTitle,
    string? Source,
    string? Notes,
    bool Success,
    string? ErrorMessage
);