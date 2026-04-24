using Logbook.Data;
using Logbook.Models;
using Logbook.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Logbook.Tests;

public class FilteringServiceTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static JobApplication Application(
        string company, string role,
        ApplicationStatus status = ApplicationStatus.Applied)
    {
        return new JobApplication
        {
            CompanyName = company,
            RoleTitle = role,
            DateApplied = DateTime.Today,
            Status = status
        };
    }

    // No filters — all records returned
    [Fact]
    public async Task GetFilteredAsync_NoFilters_ReturnsAll()
    {
        using var context = CreateContext(nameof(GetFilteredAsync_NoFilters_ReturnsAll));
        var service = new JobApplicationService(context);

        await service.AddAsync(Application("Acme Ltd", "Developer"));
        await service.AddAsync(Application("Beta Corp", "Designer"));

        var results = await service.GetFilteredAsync(null, null);

        Assert.Equal(2, results.Count());
    }

    // Filter by status — only matching status returned
    [Fact]
    public async Task GetFilteredAsync_ByStatus_ReturnsMatchingOnly()
    {
        using var context = CreateContext(nameof(GetFilteredAsync_ByStatus_ReturnsMatchingOnly));
        var service = new JobApplicationService(context);

        await service.AddAsync(Application("Acme Ltd", "Developer", ApplicationStatus.Applied));
        await service.AddAsync(Application("Beta Corp", "Designer", ApplicationStatus.Rejected));

        var results = await service.GetFilteredAsync(ApplicationStatus.Applied, null);

        Assert.Single(results);
        Assert.Equal("Acme Ltd", results.First().CompanyName);
    }

    // Search by company name — case insensitive
    [Fact]
    public async Task GetFilteredAsync_ByCompanyName_ReturnsCaseInsensitiveMatch()
    {
        using var context = CreateContext(nameof(GetFilteredAsync_ByCompanyName_ReturnsCaseInsensitiveMatch));
        var service = new JobApplicationService(context);

        await service.AddAsync(Application("Acme Ltd", "Developer"));
        await service.AddAsync(Application("Beta Corp", "Designer"));

        var results = await service.GetFilteredAsync(null, "acme");

        Assert.Single(results);
        Assert.Equal("Acme Ltd", results.First().CompanyName);
    }

    // Search by role title — case insensitive
    [Fact]
    public async Task GetFilteredAsync_ByRoleTitle_ReturnsCaseInsensitiveMatch()
    {
        using var context = CreateContext(nameof(GetFilteredAsync_ByRoleTitle_ReturnsCaseInsensitiveMatch));
        var service = new JobApplicationService(context);

        await service.AddAsync(Application("Acme Ltd", "Senior Developer"));
        await service.AddAsync(Application("Beta Corp", "Designer"));

        var results = await service.GetFilteredAsync(null, "developer");

        Assert.Single(results);
        Assert.Equal("Acme Ltd", results.First().CompanyName);
    }

    // Both filters combined — only records matching both are returned
    [Fact]
    public async Task GetFilteredAsync_ByStatusAndSearch_ReturnsCombinedMatch()
    {
        using var context = CreateContext(nameof(GetFilteredAsync_ByStatusAndSearch_ReturnsCombinedMatch));
        var service = new JobApplicationService(context);

        await service.AddAsync(Application("Acme Ltd", "Developer", ApplicationStatus.Applied));
        await service.AddAsync(Application("Acme Retail", "Manager", ApplicationStatus.Rejected));
        await service.AddAsync(Application("Beta Corp", "Developer", ApplicationStatus.Applied));

        var results = await service.GetFilteredAsync(ApplicationStatus.Applied, "acme");

        Assert.Single(results);
        Assert.Equal("Acme Ltd", results.First().CompanyName);
    }

    // Search with no matches — empty result
    [Fact]
    public async Task GetFilteredAsync_SearchWithNoMatches_ReturnsEmpty()
    {
        using var context = CreateContext(nameof(GetFilteredAsync_SearchWithNoMatches_ReturnsEmpty));
        var service = new JobApplicationService(context);

        await service.AddAsync(Application("Acme Ltd", "Developer"));

        var results = await service.GetFilteredAsync(null, "zzz");

        Assert.Empty(results);
    }

    // Status filter with no matches — empty result
    [Fact]
    public async Task GetFilteredAsync_StatusWithNoMatches_ReturnsEmpty()
    {
        using var context = CreateContext(nameof(GetFilteredAsync_StatusWithNoMatches_ReturnsEmpty));
        var service = new JobApplicationService(context);

        await service.AddAsync(Application("Acme Ltd", "Developer", ApplicationStatus.Applied));

        var results = await service.GetFilteredAsync(ApplicationStatus.OfferReceived, null);

        Assert.Empty(results);
    }

    // Results are ordered by DateApplied descending
    [Fact]
    public async Task GetFilteredAsync_ReturnsResultsOrderedByDateDescending()
    {
        using var context = CreateContext(nameof(GetFilteredAsync_ReturnsResultsOrderedByDateDescending));
        var service = new JobApplicationService(context);

        var app1 = Application("First Co", "Developer");
        app1.DateApplied = DateTime.Today.AddDays(-2);
        var app2 = Application("Second Co", "Developer");
        app2.DateApplied = DateTime.Today;

        await service.AddAsync(app1);
        await service.AddAsync(app2);

        var results = (await service.GetFilteredAsync(null, null)).ToList();

        Assert.Equal("Second Co", results[0].CompanyName);
        Assert.Equal("First Co", results[1].CompanyName);
    }

    [Fact]
    public async Task GetFilteredAsync_SortByCompany_ReturnsAscendingOrder()
    {
        var ctx = CreateContext("SortTest1");
        var svc = new JobApplicationService(ctx);
        await svc.AddAsync(new JobApplication { CompanyName = "Zebra", RoleTitle = "Dev", DateApplied = DateTime.Today, Status = ApplicationStatus.Applied });
        await svc.AddAsync(new JobApplication { CompanyName = "Acme", RoleTitle = "Dev", DateApplied = DateTime.Today, Status = ApplicationStatus.Applied });
        var result = (await svc.GetFilteredAsync(null, null, "company")).ToList();
        Assert.Equal("Acme", result[0].CompanyName);
    }

    [Fact]
    public async Task GetFilteredAsync_SortByCompanyDesc_ReturnsDescendingOrder()
    {
        var ctx = CreateContext("SortTest2");
        var svc = new JobApplicationService(ctx);
        await svc.AddAsync(new JobApplication { CompanyName = "Zebra", RoleTitle = "Dev", DateApplied = DateTime.Today, Status = ApplicationStatus.Applied });
        await svc.AddAsync(new JobApplication { CompanyName = "Acme", RoleTitle = "Dev", DateApplied = DateTime.Today, Status = ApplicationStatus.Applied });
        var result = (await svc.GetFilteredAsync(null, null, "company_desc")).ToList();
        Assert.Equal("Zebra", result[0].CompanyName);
    }

}