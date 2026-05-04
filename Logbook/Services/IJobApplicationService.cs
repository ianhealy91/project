using Logbook.Models;

namespace Logbook.Services;

/// <summary>
/// Defines data operations for job application entries.
/// </summary>

public interface IJobApplicationService
{
    /// <summary>
    /// Returns all job applications ordered by date applied descending.
    /// </summary>
    Task<IEnumerable<JobApplication>> GetAllAsync();
    /// <summary>
    /// Returns applications submitted within the specified date range, ordered by date descending.
    /// </summary>
    Task<IEnumerable<JobApplication>> GetByDateRangeAsync(DateTime from, DateTime to);
    /// <summary>
    /// Returns applications matching the optional status filter and keyword search,
    /// ordered by the specified column. Defaults to date applied descending.
    /// </summary>
    Task<IEnumerable<JobApplication>> GetFilteredAsync(ApplicationStatus? status, string? search, string? sortBy = null);
    /// <summary>
    /// Returns a paginated subset of filtered applications.
    /// </summary>
    Task<PagedResult<JobApplication>> GetPagedAsync(ApplicationStatus? status, string? search, string? sortBy, int page, int pageSize);
    /// <summary>
    /// Returns a single application by ID, or null if not found.
    /// </summary>
    Task<JobApplication?> GetByIdAsync(int id);
    /// <summary>
    /// Persists a new application entry and sets CreatedAt and UpdatedAt timestamps.
    /// </summary>
    Task<JobApplication> AddAsync(JobApplication application);
    /// <summary>
    /// Updates an existing application entry and refreshes the UpdatedAt timestamp.
    /// Returns null if the record does not exist.
    /// </summary>
    Task<JobApplication?> UpdateAsync(JobApplication application);
    /// <summary>
    /// Deletes an application by ID. Returns false if the record does not exist.
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
