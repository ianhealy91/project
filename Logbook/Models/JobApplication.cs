using System.ComponentModel.DataAnnotations;

namespace Logbook.Models;

public class JobApplication
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string RoleTitle { get; set; } = string.Empty;

    [Required]
    public DateTime DateApplied { get; set; }

    [MaxLength(100)]
    public string? Source { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
