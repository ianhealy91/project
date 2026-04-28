using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Logbook.Services;

public class AiExtractionService : IAiExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<AiExtractionService> _logger;

    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string Model = "claude-haiku-4-5-20251001";

    public AiExtractionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AiExtractionService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic:ApiKey is not configured.");
        _logger = logger;
    }

    public async Task<ExtractionResult> ExtractAsync(string input)
    {
        try
        {
            // If input looks like a URL, try to fetch the page content first
            string content = input;
            string? detectedSource = null;

            if (Uri.TryCreate(input.Trim(), UriKind.Absolute, out var uri)
                && (uri.Scheme == "http" || uri.Scheme == "https"))
            {
                detectedSource = DetectSource(uri.Host);
                var fetched = await TryFetchUrlAsync(uri);
                content = fetched ?? input; // Fall back to raw input if fetch fails
            }

            return await CallClaudeAsync(content, detectedSource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI extraction failed for input: {Input}", input[..Math.Min(100, input.Length)]);
            return new ExtractionResult(null, null, null, null, false, "Extraction failed. Please fill in the fields manually.");
        }
    }

    private async Task<string?> TryFetchUrlAsync(Uri uri)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; Logbook/1.0)");
            request.Headers.Accept.ParseAdd("text/html");

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var html = await response.Content.ReadAsStringAsync();

            // Strip HTML tags and collapse whitespace for cleaner LLM input
            var text = Regex.Replace(html, "<[^>]+>", " ");
            text = Regex.Replace(text, @"\s+", " ").Trim();

            // Truncate to avoid excessive token usage — 4000 chars is plenty
            return text.Length > 4000 ? text[..4000] : text;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch URL {Uri}, falling back to raw input", uri);
            return null;
        }
    }

    private string? DetectSource(string host)
    {
        if (host.Contains("linkedin")) return "LinkedIn";
        if (host.Contains("indeed")) return "Indeed";
        if (host.Contains("irishjobs")) return "IrishJobs.ie";
        if (host.Contains("jobs.ie")) return "Jobs.ie";
        if (host.Contains("glassdoor")) return "Glassdoor";
        return null;
    }

    private async Task<ExtractionResult> CallClaudeAsync(string content, string? detectedSource)
    {
        var sourceHint = detectedSource != null ? $"The listing was found on: {detectedSource}" : "";
        var prompt = "Extract job listing information from the following content.\n" +
                     "Return ONLY a valid JSON object with exactly these fields, no other text, no markdown:\n" +
                     "{\n" +
                     "  \"companyName\": \"string or null if not found\",\n" +
                     "  \"roleTitle\": \"string or null if not found\",\n" +
                     "  \"source\": \"string or null - the job board or company site e.g. LinkedIn, Indeed, IrishJobs.ie\",\n" +
                     "  \"notes\": \"string - a 2-3 sentence summary of key requirements and responsibilities\"\n" +
                     "}\n\n" +
                     sourceHint + "\n\n" +
                     "Content:\n" + content;

        var requestBody = new
        {
            model = Model,
            max_tokens = 512,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Anthropic API returned {StatusCode}: {Body}", response.StatusCode, responseBody);
            return new ExtractionResult(null, null, null, null, false, "AI service unavailable. Please fill in the fields manually.");
        }

        // Parse the Anthropic response envelope
        using var doc = JsonDocument.Parse(responseBody);
        var text = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "";

        // Strip markdown code fences if Claude wrapped the response
        var json = text.Trim();
        if (json.StartsWith("```"))
        {
            json = Regex.Replace(json, @"^```[a-z]*\n?", "", RegexOptions.Multiline);
            json = json.Replace("```", "").Trim();
        }

        // Parse the extracted JSON from Claude's response
        using var extracted = JsonDocument.Parse(json);

        var root = extracted.RootElement;

        string? Get(string key) =>
            root.TryGetProperty(key, out var val) && val.ValueKind != JsonValueKind.Null
                ? val.GetString()
                : null;

        return new ExtractionResult(
            CompanyName: Get("companyName"),
            RoleTitle: Get("roleTitle"),
            Source: detectedSource ?? Get("source"),
            Notes: Get("notes"),
            Success: true,
            ErrorMessage: null
        );
    }
}