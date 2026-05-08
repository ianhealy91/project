# Logbook

A web-based job application tracker built with ASP.NET Core MVC and C#, designed to help jobseekers log, manage, and report on their job search activity. Includes support for generating structured PDF reports suitable for submission to the Department of Social Protection (DSP) under Ireland's Jobseeker's Allowance scheme.

---

## Features

- Full CRUD management of job application entries
- Five-stage status workflow: Applied, Interview Scheduled, Offer Received, Rejected, Withdrawn
- Application list with filtering, keyword search, column sorting, and pagination
- Inline status updates from the application list
- Follow-up date field with overdue and due-today indicators
- Dashboard with date range filtering and status breakdown
- PDF export via PdfSharpCore for DSP compliance reporting
- AI-assisted auto-fill from job listing URL or pasted text (via Anthropic Claude Haiku)
- OpenTelemetry instrumentation with Grafana Cloud export
- Unsaved changes warning on Create and Edit forms

---

## Prerequisites

The following must be installed before running the application:

| Dependency | Version | Download |
|---|---|---|
| .NET SDK | 8.0.x (LTS) | https://dotnet.microsoft.com/download/dotnet/8 |
| Git | 2.x | https://git-scm.com/downloads |

**Optional: required only for database migrations:**

| Dependency | Install command |
|---|---|
| EF Core CLI tools | `dotnet tool install --global dotnet-ef` |

**Optional: required only for AI auto-fill feature:**

- An Anthropic API key (https://console.anthropic.com)

---

## Getting started

### 1. Clone the repository

```bash
git clone https://github.com/ianhealy91/project.git
cd project
```

### 2. Restore dependencies

```bash
dotnet restore Logbook.sln
```

### 3. Configure the application

The application runs without any configuration for basic functionality. The SQLite database is created automatically on first run at:

```
%LOCALAPPDATA%\Logbook\logbook.db        # Windows
~/.local/share/Logbook/logbook.db        # Linux / macOS
```

#### Optional: AI auto-fill feature

To enable the AI-assisted job listing extraction feature, create `Logbook/appsettings.Development.json` with your Anthropic API key:

```json
{
  "Anthropic": {
    "ApiKey": "sk-ant-your-key-here"
  }
}
```

> ? Do not commit this file. It is listed in `.gitignore`.

#### Optional: Grafana Cloud observability

To enable OpenTelemetry export to Grafana Cloud, add the following to `appsettings.Development.json`:

```json
{
  "Anthropic": {
    "ApiKey": "sk-ant-your-key-here"
  },
  "Grafana": {
    "OtlpEndpoint": "https://otlp-gateway-prod-xx-xxxx-x.grafana.net/otlp",
    "InstanceId": "your-instance-id",
    "ApiToken": "your-grafana-token"
  }
}
```

If Grafana credentials are not present the application starts normally without telemetry.

### 4. Run the application

```bash
cd Logbook
dotnet run
```

The application will be available at `https://localhost:{port}` (the exact port is shown in the console output).

### 5. Run the tests

```bash
dotnet test Logbook.sln
```

All 29 unit tests should pass. No external dependencies are required to run the tests, an in-memory database is used automatically.

---

## API documentation

The OpenAPI specification is available at `/swagger` when running in development mode:

```
https://localhost:{port}/swagger
```

---

## Project structure

```
Logbook/
??? Controllers/          # ApplicationsController, DashboardController
??? Data/                 # AppDbContext, EF Core configuration
??? Migrations/           # EF Core migration files
??? Models/               # JobApplication entity, ApplicationStatus enum, PagedResult
??? Services/             # IJobApplicationService, JobApplicationService,
?                         # PdfExportService, IAiExtractionService, AiExtractionService
??? ViewModels/           # AddEditViewModel, DashboardViewModel
??? Views/                # Razor views organised by controller
??? wwwroot/              # CSS, static assets

Logbook.Tests/
??? JobApplicationServiceTests.cs
??? DashboardServiceTests.cs
??? FilteringServiceTests.cs
```

---

## CI/CD pipeline

The repository uses GitHub Actions for CI/CD:

| Workflow | Trigger | Purpose |
|---|---|---|
| `ci.yml` | Push / PR to `main` or `develop` | Build, test, security scan, publish, SBOM, Super-Linter |
| `cd.yml` | CI success on `main` | Deploy to staging and production with approval gates |
| `codeql.yml` | Weekly schedule + push to `main` | CodeQL SAST analysis |

Pipeline results and artefacts are available in the GitHub Actions tab. CodeQL findings surface in the Security ? Code scanning tab.

---

## Notes

- Direct LinkedIn job URLs are not supported by the AI auto-fill feature due to access restrictions. Paste the job description text instead.
- The SQLite database file is stored outside the repository and is never committed to version control.
- All secrets (Anthropic API key, Grafana credentials) are stored as .NET user secrets in development and as GitHub Encrypted Secrets in the CI/CD pipeline.

