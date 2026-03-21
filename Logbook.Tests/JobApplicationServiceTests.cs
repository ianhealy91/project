using Xunit;
using Microsoft.EntityFrameworkCore;
using Logbook.Data;
using Logbook.Models;
using Logbook.Services;

namespace Logbook.Tests;

/// <summary>
/// Unit tests for JobApplicationService.
/// Each test uses a fresh in-memory database to ensure full isolation.
/// </summary>
public class JobApplicationServiceTests
{
    // ── Helpers ──────────────────────────────────────────────────────────

    private static AppDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static JobApplication SampleApplication(
        string company = "Acme Ltd",
        string role = "Software Developer",
        ApplicationStatus status = ApplicationStatus.Applied)
    {
        return new JobApplication
        {
            CompanyName = company,
            RoleTitle = role,
            DateApplied = DateTime.Today,
            Source = "LinkedIn",
            Status = status,
            Notes = "Applied via online portal."
        };
    }

    // ── AddAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ShouldPersistApplication()
    {
        using var context = CreateInMemoryContext(nameof(AddAsync_ShouldPersistApplication));
        var service = new JobApplicationService(context);

        var application = SampleApplication();
        var result = await service.AddAsync(application);

        Assert.NotEqual(0, result.Id);
        Assert.Single(await service.GetAllAsync());
    }

    [Fact]
    public async Task AddAsync_ShouldSetCreatedAtAndUpdatedAt()
    {
        using var context = CreateInMemoryContext(nameof(AddAsync_ShouldSetCreatedAtAndUpdatedAt));
        var service = new JobApplicationService(context);

        var before = DateTime.UtcNow.AddSeconds(-1);
        var result = await service.AddAsync(SampleApplication());
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(result.CreatedAt, before, after);
        Assert.InRange(result.UpdatedAt, before, after);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllApplications()
    {
        using var context = CreateInMemoryContext(nameof(GetAllAsync_ShouldReturnAllApplications));
        var service = new JobApplicationService(context);

        await service.AddAsync(SampleApplication("Company A", "Role A"));
        await service.AddAsync(SampleApplication("Company B", "Role B"));
        await service.AddAsync(SampleApplication("Company C", "Role C"));

        var results = await service.GetAllAsync();

        Assert.Equal(3, results.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyWhenNoApplications()
    {
        using var context = CreateInMemoryContext(nameof(GetAllAsync_ShouldReturnEmptyWhenNoApplications));
        var service = new JobApplicationService(context);

        var results = await service.GetAllAsync();

        Assert.Empty(results);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectApplication()
    {
        using var context = CreateInMemoryContext(nameof(GetByIdAsync_ShouldReturnCorrectApplication));
        var service = new JobApplicationService(context);

        var added = await service.AddAsync(SampleApplication("Target Corp", "Engineer"));

        var result = await service.GetByIdAsync(added.Id);

        Assert.NotNull(result);
        Assert.Equal("Target Corp", result.CompanyName);
        Assert.Equal("Engineer", result.RoleTitle);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullForMissingId()
    {
        using var context = CreateInMemoryContext(nameof(GetByIdAsync_ShouldReturnNullForMissingId));
        var service = new JobApplicationService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingApplication()
    {
        using var context = CreateInMemoryContext(nameof(UpdateAsync_ShouldModifyExistingApplication));
        var service = new JobApplicationService(context);

        var added = await service.AddAsync(SampleApplication());
        added.CompanyName = "Updated Corp";
        added.Status = ApplicationStatus.InterviewScheduled;

        var result = await service.UpdateAsync(added);

        Assert.NotNull(result);
        Assert.Equal("Updated Corp", result.CompanyName);
        Assert.Equal(ApplicationStatus.InterviewScheduled, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNullForMissingId()
    {
        using var context = CreateInMemoryContext(nameof(UpdateAsync_ShouldReturnNullForMissingId));
        var service = new JobApplicationService(context);

        var ghost = SampleApplication();
        ghost.Id = 999;

        var result = await service.UpdateAsync(ghost);

        Assert.Null(result);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldRemoveApplication()
    {
        using var context = CreateInMemoryContext(nameof(DeleteAsync_ShouldRemoveApplication));
        var service = new JobApplicationService(context);

        var added = await service.AddAsync(SampleApplication());
        var deleted = await service.DeleteAsync(added.Id);

        Assert.True(deleted);
        Assert.Empty(await service.GetAllAsync());
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalseForMissingId()
    {
        using var context = CreateInMemoryContext(nameof(DeleteAsync_ShouldReturnFalseForMissingId));
        var service = new JobApplicationService(context);

        var result = await service.DeleteAsync(999);

        Assert.False(result);
    }

    // ── GetByDateRangeAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetByDateRangeAsync_ShouldReturnOnlyApplicationsInRange()
    {
        using var context = CreateInMemoryContext(nameof(GetByDateRangeAsync_ShouldReturnOnlyApplicationsInRange));
        var service = new JobApplicationService(context);

        var inRange = new JobApplication
        {
            CompanyName = "In Range Ltd",
            RoleTitle = "Developer",
            DateApplied = DateTime.Today,
            Status = ApplicationStatus.Applied
        };
        var outOfRange = new JobApplication
        {
            CompanyName = "Out of Range Ltd",
            RoleTitle = "Developer",
            DateApplied = DateTime.Today.AddDays(-30),
            Status = ApplicationStatus.Applied
        };

        await service.AddAsync(inRange);
        await service.AddAsync(outOfRange);

        var results = await service.GetByDateRangeAsync(
            DateTime.Today.AddDays(-7),
            DateTime.Today);

        Assert.Single(results);
        Assert.Equal("In Range Ltd", results.First().CompanyName);
    }
}
