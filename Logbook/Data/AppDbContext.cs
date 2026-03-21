using Microsoft.EntityFrameworkCore;
using Logbook.Models;

namespace Logbook.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RoleTitle).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DateApplied).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
        });
    }
}
