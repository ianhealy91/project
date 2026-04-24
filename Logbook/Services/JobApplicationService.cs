using Microsoft.EntityFrameworkCore;
using Logbook.Data;
using Logbook.Models;

namespace Logbook.Services;

public class JobApplicationService : IJobApplicationService
{
    private readonly AppDbContext _context;

    public JobApplicationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JobApplication>> GetAllAsync()
    {
        return await _context.JobApplications
            .OrderByDescending(a => a.DateApplied)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobApplication>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _context.JobApplications
            .Where(a => a.DateApplied >= from && a.DateApplied <= to)
            .OrderByDescending(a => a.DateApplied)
            .ToListAsync();
    }
    public async Task<IEnumerable<JobApplication>> GetFilteredAsync(
    ApplicationStatus? status, string? search, string? sortBy = null)
    {
        var query = _context.JobApplications.AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a =>
                a.CompanyName.ToLower().Contains(term) ||
                a.RoleTitle.ToLower().Contains(term));
        }

        query = sortBy switch
        {
            "company" => query.OrderBy(a => a.CompanyName),
            "company_desc" => query.OrderByDescending(a => a.CompanyName),
            "role" => query.OrderBy(a => a.RoleTitle),
            "role_desc" => query.OrderByDescending(a => a.RoleTitle),
            "status" => query.OrderBy(a => a.Status),
            "status_desc" => query.OrderByDescending(a => a.Status),
            "date" => query.OrderBy(a => a.DateApplied),
            _ => query.OrderByDescending(a => a.DateApplied)
        };

        return await query.ToListAsync();
    }


    public async Task<JobApplication?> GetByIdAsync(int id)
    {
        return await _context.JobApplications.FindAsync(id);
    }

    public async Task<JobApplication> AddAsync(JobApplication application)
    {
        application.CreatedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;
        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<JobApplication?> UpdateAsync(JobApplication application)
    {
        var existing = await _context.JobApplications.FindAsync(application.Id);
        if (existing is null) return null;

        existing.CompanyName = application.CompanyName;
        existing.RoleTitle = application.RoleTitle;
        existing.DateApplied = application.DateApplied;
        existing.Source = application.Source;
        existing.Status = application.Status;
        existing.Notes = application.Notes;
        existing.FollowUpDate = application.FollowUpDate;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var application = await _context.JobApplications.FindAsync(id);
        if (application is null) return false;

        _context.JobApplications.Remove(application);
        await _context.SaveChangesAsync();
        return true;
    }
}
