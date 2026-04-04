using Logbook.Data;
using Logbook.Models;
using Logbook.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Logbook.Tests;

public class DashboardServiceTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static JobApplication Application(string company, DateTime dateApplied)
    {
        return new JobApplication
        {
            CompanyName = company,
            RoleTitle = "Developer",
            DateApplied = dateApplied,
            Status = ApplicationStatus.Applied
        };
    }

    // Exactly on the start date boundary, should be included
    [Fact]
    public async Task GetByDateRangeAsync_ShouldIncludeApplicationOnStartDate()
    {
        using var context = CreateContext(nameof(GetByDateRangeAsync_ShouldIncludeApplicationOnStartDate));
        var service = new JobApplicationService(context);

        var startDate = new DateTime(2026, 4, 1);
        await service.AddAsync(Application("Boundary Start Co", startDate));

        var results = await service.GetByDateRangeAsync(startDate, startDate.AddDays(6));

        Assert.Single(results);
        Assert.Equal("Boundary Start Co", results.First().CompanyName);
    }

    // Exactly on the end date boundary, should be included
    [Fact]
    public async Task GetByDateRangeAsync_ShouldIncludeApplicationOnEndDate()
    {
        using var context = CreateContext(nameof(GetByDateRangeAsync_ShouldIncludeApplicationOnEndDate));
        var service = new JobApplicationService(context);

        var endDate = new DateTime(2026, 4, 7);
        await service.AddAsync(Application("Boundary End Co", endDate));

        var results = await service.GetByDateRangeAsync(endDate.AddDays(-6), endDate);

        Assert.Single(results);
        Assert.Equal("Boundary End Co", results.First().CompanyName);
    }

    // One day before the start, should be excluded
    [Fact]
    public async Task GetByDateRangeAsync_ShouldExcludeApplicationBeforeStartDate()
    {
        using var context = CreateContext(nameof(GetByDateRangeAsync_ShouldExcludeApplicationBeforeStartDate));
        var service = new JobApplicationService(context);

        var startDate = new DateTime(2026, 4, 1);
        await service.AddAsync(Application("Too Early Co", startDate.AddDays(-1)));

        var results = await service.GetByDateRangeAsync(startDate, startDate.AddDays(6));

        Assert.Empty(results);
    }

    // One day after the end, should be excluded
    [Fact]
    public async Task GetByDateRangeAsync_ShouldExcludeApplicationAfterEndDate()
    {
        using var context = CreateContext(nameof(GetByDateRangeAsync_ShouldExcludeApplicationAfterEndDate));
        var service = new JobApplicationService(context);

        var endDate = new DateTime(2026, 4, 7);
        await service.AddAsync(Application("Too Late Co", endDate.AddDays(1)));

        var results = await service.GetByDateRangeAsync(endDate.AddDays(-6), endDate);

        Assert.Empty(results);
    }

    // Date range with no applications at all, should return empty
    [Fact]
    public async Task GetByDateRangeAsync_ShouldReturnEmptyForRangeWithNoApplications()
    {
        using var context = CreateContext(nameof(GetByDateRangeAsync_ShouldReturnEmptyForRangeWithNoApplications));
        var service = new JobApplicationService(context);

        var results = await service.GetByDateRangeAsync(DateTime.Today, DateTime.Today.AddDays(6));

        Assert.Empty(results);
    }

    // Single-day range, only applications on that exact day returned
    [Fact]
    public async Task GetByDateRangeAsync_ShouldReturnCorrectResultsForSingleDayRange()
    {
        using var context = CreateContext(nameof(GetByDateRangeAsync_ShouldReturnCorrectResultsForSingleDayRange));
        var service = new JobApplicationService(context);

        var targetDate = new DateTime(2026, 4, 3);
        await service.AddAsync(Application("Same Day Co", targetDate));
        await service.AddAsync(Application("Different Day Co", targetDate.AddDays(1)));

        var results = await service.GetByDateRangeAsync(targetDate, targetDate);

        Assert.Single(results);
        Assert.Equal("Same Day Co", results.First().CompanyName);
    }

    // Multiple applications in range, all returned, ordered by date descending
    [Fact]
    public async Task GetByDateRangeAsync_ShouldReturnMultipleApplicationsOrderedByDateDescending()
    {
        using var context = CreateContext(nameof(GetByDateRangeAsync_ShouldReturnMultipleApplicationsOrderedByDateDescending));
        var service = new JobApplicationService(context);

        var startDate = new DateTime(2026, 4, 1);
        await service.AddAsync(Application("First Applied Co", startDate));
        await service.AddAsync(Application("Second Applied Co", startDate.AddDays(2)));
        await service.AddAsync(Application("Third Applied Co", startDate.AddDays(4)));

        var results = (await service.GetByDateRangeAsync(startDate, startDate.AddDays(6))).ToList();

        Assert.Equal(3, results.Count);
        Assert.Equal("Third Applied Co", results[0].CompanyName);
        Assert.Equal("First Applied Co", results[2].CompanyName);
    }
}