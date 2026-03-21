using Logbook.Models;

namespace Logbook.Services;

public interface IJobApplicationService
{
    Task<IEnumerable<JobApplication>> GetAllAsync();
    Task<IEnumerable<JobApplication>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<JobApplication?> GetByIdAsync(int id);
    Task<JobApplication> AddAsync(JobApplication application);
    Task<JobApplication?> UpdateAsync(JobApplication application);
    Task<bool> DeleteAsync(int id);
}
