using Logbook.Data;
using Logbook.Services;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<PdfExportService>();

// EF Core with SQLite
var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Logbook",
    "logbook.db");

Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Application services
builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();

builder.Services.AddHttpClient<AiExtractionService>();
builder.Services.AddScoped<IAiExtractionService, AiExtractionService>();

// OpenTelemetry
var otlpEndpoint = builder.Configuration["Grafana:OtlpEndpoint"];
var instanceId = builder.Configuration["Grafana:InstanceId"];
var apiToken = builder.Configuration["Grafana:ApiToken"];

if (!string.IsNullOrWhiteSpace(otlpEndpoint) &&
    !string.IsNullOrWhiteSpace(instanceId) &&
    !string.IsNullOrWhiteSpace(apiToken))
{
    var credentials = Convert.ToBase64String(
        System.Text.Encoding.UTF8.GetBytes($"{instanceId}:{apiToken}"));

    var resourceBuilder = ResourceBuilder.CreateDefault()
        .AddService(serviceName: "Logbook", serviceVersion: "1.0.0")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName.ToLower()
        });

    builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter(otlp =>
        {
            otlp.Endpoint = new Uri($"{otlpEndpoint}/v1/traces");
            otlp.Headers = $"Authorization=Basic {credentials}";
            otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        }))
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(otlp =>
        {
            otlp.Endpoint = new Uri($"{otlpEndpoint}/v1/metrics");
            otlp.Headers = $"Authorization=Basic {credentials}";
            otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        }));

    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.SetResourceBuilder(resourceBuilder);
        logging.AddOtlpExporter(otlp =>
        {
            otlp.Endpoint = new Uri($"{otlpEndpoint}/v1/logs");
            otlp.Headers = $"Authorization=Basic {credentials}";
            otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        });
    });
}

// OpenAPI / Swagger
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Logbook API v1");
        c.RoutePrefix = "swagger";
    });
}

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Applications}/{action=Index}/{id?}");

// Minimal API endpoint for Swagger visibility
app.MapPost("/api/extract", async (
    string input,
    IAiExtractionService aiService) =>
{
    if (string.IsNullOrWhiteSpace(input))
        return Results.BadRequest(new { error = "No input provided." });

    var result = await aiService.ExtractAsync(input);

    if (!result.Success)
        return Results.Ok(new { success = false, error = result.ErrorMessage });

    return Results.Ok(new
    {
        success = true,
        companyName = result.CompanyName,
        roleTitle = result.RoleTitle,
        source = result.Source,
        notes = result.Notes
    });
})
.WithName("ExtractJobListing")
.WithSummary("Extract job listing fields from a URL or pasted text")
.WithDescription("Accepts a job listing URL or raw job description text. Fetches page content where possible and calls Claude Haiku to extract company name, role title, source, and a notes summary.")
.Produces(200)
.Produces(400);

app.Run();

// Needed for test project to reference Program
public partial class Program { }